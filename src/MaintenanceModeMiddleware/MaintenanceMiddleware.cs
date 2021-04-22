using MaintenanceModeMiddleware.Configuration;
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
        private readonly IMaintenanceControlService _maintenanceCtrlSev;
        private readonly IPathMapperService _pathMapperSvc;
        private readonly OptionCollection _startupOptions;

        public MaintenanceMiddleware(RequestDelegate next,
            IMaintenanceControlService maintenanceCtrlSev,
            IPathMapperService pathMapperSvc,
            Action<IMiddlewareOptionsBuilder> optionsBuilderDelegate)
        {
            _next = next;
            _maintenanceCtrlSev = maintenanceCtrlSev;
            _pathMapperSvc = pathMapperSvc;

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
            IMaintenanceState maintenanceState = _maintenanceCtrlSev
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
                   .GetResponse(_pathMapperSvc);

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

            if (_maintenanceCtrlSev
                .GetState() is IMiddlewareOptionsContainer optionsContainer)
            {
                latestOptions = optionsContainer.MiddlewareOptions;
            }

            return  latestOptions ?? _startupOptions;
        }

        private OptionCollection GetStartupOptions(Action<MiddlewareOptionsBuilder> builderDelegate)
        {
            var optionsBuilder = new MiddlewareOptionsBuilder(_pathMapperSvc);
            builderDelegate?.Invoke(optionsBuilder);
            return optionsBuilder.GetOptions();
        }
    }
}