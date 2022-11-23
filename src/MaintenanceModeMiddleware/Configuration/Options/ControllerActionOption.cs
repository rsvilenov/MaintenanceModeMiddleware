using MaintenanceModeMiddleware.Configuration.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
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
            var tokens = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(Value.AreaName))
            {
                tokens.Add("area", Value.AreaName);
            }

            return tokens;
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
            ProcessRouteValues(routeData);
            ProcessDataTokens(routeData);

            return Task.FromResult(new PreprocessResult { CallNext = true });
        }

        private void ProcessDataTokens(RouteData routeData)
        {
            var newDataTokens = GetDataTokens();
            foreach (string newDataTokenKey in newDataTokens.Keys)
            {
                if (routeData.Values.ContainsKey(newDataTokenKey))
                {
                    routeData.Values[newDataTokenKey] = newDataTokens[newDataTokenKey];
                }
                else
                {
                    routeData.Values.Add(newDataTokenKey, newDataTokens[newDataTokenKey]);
                }
            }
        }

        private void ProcessRouteValues(RouteData routeData)
        {
            var newRouteValues = GetRouteValues();
            foreach (string newRouteValueKey in newRouteValues.Keys)
            {
                if (routeData.Values.ContainsKey(newRouteValueKey))
                {
                    routeData.Values[newRouteValueKey] = newRouteValues[newRouteValueKey];
                }
                else
                {
                    routeData.Values.Add(newRouteValueKey, newRouteValues[newRouteValueKey]);
                }
            }
        }
    }
}
