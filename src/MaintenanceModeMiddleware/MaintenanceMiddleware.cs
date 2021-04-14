using MaintenanceModeMiddleware.Configuration;
using MaintenanceModeMiddleware.Configuration.Builders;
using MaintenanceModeMiddleware.Configuration.Data;
using MaintenanceModeMiddleware.Configuration.Options;
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
        private readonly ICanOverrideMiddlewareOptions _optionsOverriderSvc;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly OptionCollection _startupOptions;

        public MaintenanceMiddleware(RequestDelegate next,
            IMaintenanceControlService maintenanceCtrlSev,
            IWebHostEnvironment webHostEnvironment,
            Action<MiddlewareOptionsBuilder> options)
        {
            _next = next;
            _maintenanceCtrlSev = maintenanceCtrlSev;
            _webHostEnvironment = webHostEnvironment;

            if (maintenanceCtrlSev is ICanOverrideMiddlewareOptions overriderSvc)
            {
                _optionsOverriderSvc = overriderSvc;
            }

            MiddlewareOptionsBuilder optionsBuilder = new MiddlewareOptionsBuilder();
            options?.Invoke(optionsBuilder);
            _startupOptions = optionsBuilder.GetOptions();

            _startupOptions
                .GetSingleOrDefault<IResponseHolder>()
                .Verify(webHostEnvironment);

            // We should try to restore the state after all dependecies have been registered,
            // because some implementation of IStateStore may rely on a dependency, such is the case
            // with FileStateStore - it relies on a resolvable instance of IWebHostEnvironment.
            // That's why we are doing this here and not, for example, in the service's constructor.
            if (_maintenanceCtrlSev is ICanRestoreState iCanRestoreState)
            {
                iCanRestoreState.RestoreState();
            }
        }

        public async Task Invoke(HttpContext context)
        {
            if (!_maintenanceCtrlSev.IsMaintenanceModeOn)
            {
                await _next.Invoke(context);
                return;
            }

            OptionCollection options = _optionsOverriderSvc
                ?.GetOptionsToOverride()
                ?? _startupOptions;

            MaintenanceResponse response = options
                .GetSingleOrDefault<IResponseHolder>()
                .GetResponse(_webHostEnvironment);

            if (options.GetAll<IContextMatcher>()
                .Any(matcher => matcher.IsMatch(context)))
            {
                await _next.Invoke(context);
                return;
            }

            context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
            context.Response.Headers.Add("Retry-After", options.GetSingleOrDefault<Code503RetryIntervalOption>().Value.ToString());
            context.Response.ContentType = response.GetContentTypeString();

            await context
                .Response
                .WriteAsync(response.ContentEncoding.GetString(response.ContentBytes),
                    response.ContentEncoding);
        }
    }
}