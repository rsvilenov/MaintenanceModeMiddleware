﻿using MaintenanceModeMiddleware.Configuration;
using MaintenanceModeMiddleware.Configuration.Builders;
using MaintenanceModeMiddleware.Configuration.Data;
using MaintenanceModeMiddleware.Configuration.State;
using MaintenanceModeMiddleware.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MaintenanceModeMiddleware
{
    internal class MaintenanceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMaintenanceControlService _maintenanceCtrlSvc;
        private readonly IDirectoryMapperService _dirMapperSvc;
        private readonly OptionCollection _startupOptions;

        public MaintenanceMiddleware(RequestDelegate next,
            IMaintenanceControlService maintenanceCtrlSvc,
            IDirectoryMapperService dirMapperSvc,
            Action<IMiddlewareOptionsBuilder> optionsBuilderDelegate)
        {
            _next = next;
            _maintenanceCtrlSvc = maintenanceCtrlSvc;
            _dirMapperSvc = dirMapperSvc;

            _startupOptions = GetStartupOptions(optionsBuilderDelegate);
        }

        public async Task Invoke(HttpContext context)
        {
            if (ShouldAllowRequest(context))
            {
                await _next.Invoke(context);
                return;
            }

            await WriteMaintenanceResponse(context);
        }

        private bool ShouldAllowRequest(HttpContext context)
        {
            IMaintenanceState maintenanceState = _maintenanceCtrlSvc
                .GetState();

            if (maintenanceState.IsMaintenanceOn)
            {
                OptionCollection options = GetLatestOptions();

                return options
                    .GetAll<IAllowedRequestMatcher>()
                    .Any(matcher => matcher.IsMatch(context));
            }

            return true;
        }

        private async Task WriteMaintenanceResponse(HttpContext context)
        {
            MaintenanceResponse response = GetLatestOptions()
                   .GetSingleOrDefault<IResponseHolder>()
                   .GetResponse(_dirMapperSvc);

            context
                .Response
                .StatusCode = (int)HttpStatusCode.ServiceUnavailable;

            context
                .Response
                .Headers
                .Add("Retry-After", response.Code503RetryInterval.ToString());

            context
                .Response
                .ContentType = response.GetContentTypeString();

            string responseStr = response
                .ContentEncoding
                .GetString(response.ContentBytes);

            await context
                .Response
                .WriteAsync(responseStr,
                    response.ContentEncoding);
        }

        private OptionCollection GetLatestOptions()
        {
            OptionCollection latestOptions = null;

            if (_maintenanceCtrlSvc
                .GetState() is IMiddlewareOptionsContainer optionsContainer)
            {
                latestOptions = optionsContainer.MiddlewareOptions;
            }

            return  latestOptions ?? _startupOptions;
        }

        private OptionCollection GetStartupOptions(Action<MiddlewareOptionsBuilder> builderDelegate)
        {
            var optionsBuilder = new MiddlewareOptionsBuilder(_dirMapperSvc);
            builderDelegate?.Invoke(optionsBuilder);
            return optionsBuilder.GetOptions();
        }
    }
}