using MaintenanceModeMiddleware.Configuration.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceModeMiddleware.Configuration.Options
{
    internal class ControllerActionOption : Option<ControllerActionData>, 
        IRouteDataModifier,
        IRequestHandler
    {
        private const string PARTS_SEPARATOR = ":";

        public Dictionary<string, string> GetDataTokens()
        {
            return new Dictionary<string, string>
            {
                { "area", Value.AreaName }
            };
        }

        public Dictionary<string, string> GetRouteValues()
        {
            return new Dictionary<string, string>
            {
                { "controller", Value.ControllerName },
                { "action", Value.ActionName }
            };
        }

        public override string GetStringValue()
        {
            return $"{Value.AreaName}{PARTS_SEPARATOR}{Value.ControllerName}{PARTS_SEPARATOR}{Value.ActionName}";
        }

        public override void LoadFromString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentNullException(nameof(str));
            }

            string[] parts = str.Split(PARTS_SEPARATOR);
            Value = new ControllerActionData
            {
                AreaName = parts[0],
                ControllerName = parts[1],
                ActionName = parts[2]
            };
        }

        public ResponseStatusCodeData StatusCodeData => Value.StatusCodeData;

        Task IRequestHandler.Postprocess(HttpContext context)
        {
            if (Value.StatusCodeData.Set503StatusCode)
            {
                context.Response
                        .StatusCode = StatusCodes.Status503ServiceUnavailable;

                context.Response
                    .Headers
                    .Add("Retry-After", Value.StatusCodeData.Code503RetryInterval.ToString());
            }

            return Task.CompletedTask;
        }

        Task<PreprocessResult> IRequestHandler.Preprocess(HttpContext context)
        {
            var routeData = context.GetRouteData();

            var newRouteValues = GetRouteValues();
            foreach (string routeValueKey in routeData.Values.Keys
                .Where(key => newRouteValues.ContainsKey(key)))
            {
                routeData.Values[routeValueKey] = newRouteValues[routeValueKey];
            }


            var newDataTokens = GetDataTokens();
            foreach (string dataTokenKey in routeData.DataTokens.Keys
                .Where(key => newRouteValues.ContainsKey(key)))
            {
                routeData.DataTokens[dataTokenKey] = newRouteValues[dataTokenKey];
            }

            return Task.FromResult(new PreprocessResult { CallNext = true });
        }
    }
}
