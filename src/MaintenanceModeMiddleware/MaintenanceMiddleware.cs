using MaintenanceModeMiddleware.Configuration;
using MaintenanceModeMiddleware.Configuration.State;
using MaintenanceModeMiddleware.RequestHandlers;
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
        private readonly IEnumerable<IRequestHandler> _requestHandlers;

        public MaintenanceMiddleware(RequestDelegate next,
            IMaintenanceControlService maintenanceCtrlSvc,
            IMaintenanceOptionsService optionsService,
            IEnumerable<IRequestHandler> requestHandlers)
        {
            _next = next;
            _maintenanceCtrlSvc = maintenanceCtrlSvc;
            _optionsService = optionsService;
            _requestHandlers = requestHandlers;
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

            if (ShouldAllowRequest(context))
            {
                await _next.Invoke(context);

                if (GetLatestOptions().Any<IRedirectInitializer>())
                {
                    foreach (IRequestHandler requestHandler in _requestHandlers)
                    {
                        if (requestHandler.ShouldApply(context))
                        {
                            await requestHandler.Postprocess(context);
                        }
                    }
                }

                return;
            }

            foreach (IRequestHandler requestHandler in _requestHandlers)
            {
                if (requestHandler.ShouldApply(context))
                {
                    PreprocessResult ppResult = await requestHandler.Preprocess(context);
                    if (!ppResult.CallNext)
                    {
                        return;
                    }
                }
            }

            await _next.Invoke(context);

            foreach (IRequestHandler requestHandler in _requestHandlers)
            {
                if (requestHandler.ShouldApply(context))
                {
                    await requestHandler.Postprocess(context);
                }
            }
        }

        private bool ShouldAllowRequest(HttpContext context)
        {
            OptionCollection options = GetLatestOptions();

            return options
                .GetAll<IAllowedRequestMatcher>()
                .Any(matcher => matcher.IsMatch(context));
        }

        private OptionCollection GetLatestOptions()
        {
            return _optionsService.GetOptions();
        }
    }
}