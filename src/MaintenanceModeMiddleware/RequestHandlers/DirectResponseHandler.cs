using MaintenanceModeMiddleware.Configuration;
using MaintenanceModeMiddleware.Configuration.Data;
using MaintenanceModeMiddleware.Services;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MaintenanceModeMiddleware.RequestHandlers
{
    internal class DirectResponseHandler : IRequestHandler
    {
        private readonly IDirectoryMapperService _directoryMapperService;
        private readonly IResponseHolder _responseHolder;
        public DirectResponseHandler(IMaintenanceOptionsService optionsService, IDirectoryMapperService directoryMapperService)
        {
            _directoryMapperService = directoryMapperService;

            _responseHolder = optionsService
                .GetOptions()
                .GetSingleOrDefault<IResponseHolder>();
        }

        public Task Postprocess(HttpContext context)
        {
            return Task.CompletedTask;
        }

        public async Task<PreprocessResult> Preprocess(HttpContext context)
        {
            MaintenanceResponse response = _responseHolder
                .GetResponse(_directoryMapperService);

            context
                .Response
                .StatusCode = StatusCodes.Status503ServiceUnavailable;

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

            return new PreprocessResult { CallNext = false };
        }

        public bool ShouldApply(HttpContext context)
        {
            return _responseHolder != null;
        }
    }
}
