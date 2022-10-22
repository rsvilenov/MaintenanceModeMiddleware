using MaintenanceModeMiddleware.Configuration;
using MaintenanceModeMiddleware.Configuration.Builders;
using MaintenanceModeMiddleware.Configuration.Data;
using MaintenanceModeMiddleware.Configuration.Options;
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
                PostProcessResponse(context);
                return;
            }

            await HandleMaintenanceResponse(context);
        }

        private void PostProcessResponse(HttpContext context)
        {
            PathRedirectOption pathRedirectOption = GetLatestOptions()
                .GetSingleOrDefault<PathRedirectOption>();

            if (pathRedirectOption == null)
            {
                return;
            }

            var matcher = (IAllowedRequestMatcher)pathRedirectOption;

            if (matcher.IsMatch(context)
                && pathRedirectOption.Value.Set503ResponseCode)
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

            context
                .Response
                .Redirect(redirectInitializer
                    .RedirectLocation);
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

        private OptionCollection GetStartupOptions(Action<MiddlewareOptionsBuilder> builderDelegate)
        {
            var optionsBuilder = new MiddlewareOptionsBuilder(_dirMapperSvc);
            builderDelegate?.Invoke(optionsBuilder);
            return optionsBuilder.GetOptions();
        }
    }
}