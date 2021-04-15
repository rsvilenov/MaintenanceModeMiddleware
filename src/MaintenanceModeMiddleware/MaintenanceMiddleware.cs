using MaintenanceModeMiddleware.Configuration;
using MaintenanceModeMiddleware.Configuration.Builders;
using MaintenanceModeMiddleware.Configuration.Data;
using MaintenanceModeMiddleware.Configuration.Options;
using MaintenanceModeMiddleware.Configuration.State;
using MaintenanceModeMiddleware.Services;
using Microsoft.AspNetCore.Hosting;
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
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly OptionCollection _startupOptions;

        public MaintenanceMiddleware(RequestDelegate next,
            IMaintenanceControlService maintenanceCtrlSev,
            IWebHostEnvironment webHostEnvironment,
            Action<MiddlewareOptionsBuilder> optionsBuilderDelegate)
        {
            _next = next;
            _maintenanceCtrlSev = maintenanceCtrlSev;
            _webHostEnvironment = webHostEnvironment;

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
            MaintenanceState maintenanceState = _maintenanceCtrlSev
                .GetState();

            if (maintenanceState.IsMaintenanceOn)
            {
                OptionCollection options = GetCurrentOptions();

                return options.GetAll<IAllowedRequestMatcher>()
                    .Any(matcher => matcher.IsMatch(context));
            }

            return true;
        }

        private OptionCollection GetCurrentOptions()
        {
            return _maintenanceCtrlSev
                .GetState()
                .MiddlewareOptions
                ?? _startupOptions;
        }

        private async Task WriteMaintenanceResponse(HttpContext context)
        {
            MaintenanceResponse response = GetCurrentOptions()
                   .GetSingleOrDefault<IResponseHolder>()
                   .GetResponse(_webHostEnvironment);

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


        private OptionCollection GetStartupOptions(Action<MiddlewareOptionsBuilder> builderDelegate)
        {
            var optionsBuilder = new MiddlewareOptionsBuilder(_webHostEnvironment);
            builderDelegate?.Invoke(optionsBuilder);
            return optionsBuilder.GetOptions();
        }
    }
}