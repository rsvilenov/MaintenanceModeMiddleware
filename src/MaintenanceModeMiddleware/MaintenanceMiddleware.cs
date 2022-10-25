using MaintenanceModeMiddleware.Configuration;
using MaintenanceModeMiddleware.Configuration.Builders;
using MaintenanceModeMiddleware.Configuration.Data;
using MaintenanceModeMiddleware.Configuration.Options;
using MaintenanceModeMiddleware.Configuration.State;
using MaintenanceModeMiddleware.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
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
            OptionCollection startupOptions)
        {
            _next = next;
            _maintenanceCtrlSvc = maintenanceCtrlSvc;
            _dirMapperSvc = dirMapperSvc;

            _startupOptions = startupOptions;
        }

        public async Task Invoke(HttpContext context)
        {
            if (ShouldAllowRequest(context))
            {
                await _next.Invoke(context);
                PostProcessResponse(context);
                return;
            }

            await HandleMaintenanceResponse(context);
        }

        private void PostProcessResponse(HttpContext context)
        {
            IMaintenanceState maintenanceState = _maintenanceCtrlSvc
                   .GetState();

            if (!maintenanceState.IsMaintenanceOn)
            {
                return;
            }

            PathRedirectOption pathRedirectOption = GetLatestOptions()
                .GetSingleOrDefault<PathRedirectOption>();

            if (pathRedirectOption == null)
            {
                return;
            }

            var matcher = (IAllowedRequestMatcher)pathRedirectOption;

            if (matcher.IsMatch(context)
                && pathRedirectOption.Value.Set503StatusCode)
            {
                context
                    .Response
                    .StatusCode = (int)HttpStatusCode.ServiceUnavailable;

                context
                    .Response
                    .Headers
                    .Add("Retry-After", pathRedirectOption.Value.Code503RetryInterval.ToString());
            }
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

        private async Task HandleMaintenanceResponse(HttpContext context)
        {
            IResponseHolder responseHolder = GetLatestOptions()
                   .GetSingleOrDefault<IResponseHolder>();

            if (responseHolder != null)
            {
                await WriteMaintenanceResponse(context, responseHolder);
                return;
            }

            IRedirectInitializer redirectInitializer = GetLatestOptions()
                .GetSingleOrDefault<IRedirectInitializer>();

            if (redirectInitializer != null)
            {
                context
                    .Response
                    .Redirect(redirectInitializer
                        .RedirectLocation);

                return;
            }

            IRouteDataModifier routeValuesModifier = GetLatestOptions()
                .GetSingleOrDefault<IRouteDataModifier>();

            ModifyRouteData(context, routeValuesModifier);
            await _next.Invoke(context);
        }

        private static void ModifyRouteData(HttpContext context, IRouteDataModifier routeValuesModifier)
        {
            var routeData = context.GetRouteData();

            var newRouteValues = routeValuesModifier.GetRouteValues();
            foreach (string routeValueKey in routeData.Values.Keys
                .Where(key => newRouteValues.ContainsKey(key)))
            {
                routeData.Values[routeValueKey] = newRouteValues[routeValueKey];
            }


            var newDataTokens = routeValuesModifier.GetDataTokens();
            foreach (string dataTokenKey in routeData.DataTokens.Keys
                .Where(key => newRouteValues.ContainsKey(key)))
            {
                routeData.DataTokens[dataTokenKey] = newRouteValues[dataTokenKey];
            }
        }

        private async Task WriteMaintenanceResponse(HttpContext context, IResponseHolder responseHolder)
        {
            MaintenanceResponse response = responseHolder
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
    }
}