using MaintenanceModeMiddleware.Configuration;
using MaintenanceModeMiddleware.Configuration.Data;
using MaintenanceModeMiddleware.Configuration.State;
using MaintenanceModeMiddleware.Services;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceModeMiddleware
{
    internal class MaintenanceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMaintenanceControlService _maintenanceCtrlSvc;
        private readonly IMaintenanceOptionsService _optionsService;

        public MaintenanceMiddleware(RequestDelegate next,
            IMaintenanceControlService maintenanceCtrlSvc,
            IMaintenanceOptionsService optionsService)
        {
            _next = next;
            _maintenanceCtrlSvc = maintenanceCtrlSvc;
            _optionsService = optionsService;
        }

        public async Task Invoke(HttpContext context)
        {
            IMaintenanceState maintenanceState = _maintenanceCtrlSvc
                   .GetState();

            if (!maintenanceState.IsMaintenanceOn)
            {
                await _next.Invoke(context);
                return;
            }

            IRequestHandler requestHandler = _optionsService
                .GetOptions()
                .GetSingleOrDefault<IRequestHandler>();

            if (ShouldAllowRequest(context))
            {
                await _next.Invoke(context);

                if (requestHandler is IRedirectInitializer)
                {
                    await requestHandler.Postprocess(context);
                }

                return;
            }

            PreprocessResult ppResult = await requestHandler.Preprocess(context);
            if (!ppResult.CallNext)
            {
                return;
            }

            await _next.Invoke(context);

            await requestHandler.Postprocess(context);
        }

        private bool ShouldAllowRequest(HttpContext context)
        {
            return _optionsService
                .GetOptions()
                .GetAll<IAllowedRequestMatcher>()
                .Any(matcher => matcher.IsMatch(context));
        }
    }
}