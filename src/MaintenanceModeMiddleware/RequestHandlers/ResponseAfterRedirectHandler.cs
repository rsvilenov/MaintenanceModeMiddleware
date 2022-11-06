using MaintenanceModeMiddleware.Configuration;
using MaintenanceModeMiddleware.Configuration.Options;
using MaintenanceModeMiddleware.Services;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MaintenanceModeMiddleware.RequestHandlers
{
    internal class ResponseAfterRedirectHandler : IRequestHandler
    {
        private readonly PathRedirectOption _pathRedirectOption;
        public ResponseAfterRedirectHandler(IMaintenanceOptionsService optionsService)
        {
            _pathRedirectOption = optionsService
                .GetOptions()
                .GetSingleOrDefault<PathRedirectOption>();
        }

        public Task Postprocess(HttpContext context)
        {
            context
                .Response
                .StatusCode = StatusCodes.Status503ServiceUnavailable;

            context
                .Response
                .Headers
                .Add("Retry-After", _pathRedirectOption.Value.StatusCodeData.Code503RetryInterval.ToString());

            return Task.CompletedTask;
        }

        public Task<PreprocessResult> Preprocess(HttpContext context)
        {
            return Task.FromResult(new PreprocessResult { CallNext = true });
        }

        public bool ShouldApply(HttpContext context)
        {
            if (_pathRedirectOption != null 
                && _pathRedirectOption is IAllowedRequestMatcher matcher)
            {
                return matcher.IsMatch(context)
                    && _pathRedirectOption.Value.StatusCodeData.Set503StatusCode;
            }

            return false;
        }
    }
}
