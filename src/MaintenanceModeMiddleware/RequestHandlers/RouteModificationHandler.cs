using MaintenanceModeMiddleware.Configuration;
using MaintenanceModeMiddleware.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceModeMiddleware.RequestHandlers
{
    internal class RouteModificationHandler : IRequestHandler
    {
        private readonly IRouteDataModifier _routeDataModifier;
        public RouteModificationHandler(IMaintenanceOptionsService optionsService)
        {
            _routeDataModifier = optionsService
                .GetOptions()
                .GetSingleOrDefault<IRouteDataModifier>();
        }

        public Task Postprocess(HttpContext context)
        {
            if (_routeDataModifier.StatusCodeData.Set503StatusCode)
            {
                context.Response
                        .StatusCode = StatusCodes.Status503ServiceUnavailable;

                context.Response
                    .Headers
                    .Add("Retry-After", _routeDataModifier.StatusCodeData.Code503RetryInterval.ToString());
            }

            return Task.CompletedTask;
        }

        public Task<PreprocessResult> Preprocess(HttpContext context)
        {
            var routeData = context.GetRouteData();

            var newRouteValues = _routeDataModifier.GetRouteValues();
            foreach (string routeValueKey in routeData.Values.Keys
                .Where(key => newRouteValues.ContainsKey(key)))
            {
                routeData.Values[routeValueKey] = newRouteValues[routeValueKey];
            }


            var newDataTokens = _routeDataModifier.GetDataTokens();
            foreach (string dataTokenKey in routeData.DataTokens.Keys
                .Where(key => newRouteValues.ContainsKey(key)))
            {
                routeData.DataTokens[dataTokenKey] = newRouteValues[dataTokenKey];
            }

            return Task.FromResult(new PreprocessResult { CallNext = true });
        }

        public bool ShouldApply(HttpContext context)
        {
            return _routeDataModifier != null;
        }
    }
}
