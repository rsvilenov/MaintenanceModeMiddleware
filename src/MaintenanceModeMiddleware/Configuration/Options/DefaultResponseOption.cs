using MaintenanceModeMiddleware.Configuration.Data;
using MaintenanceModeMiddleware.Configuration.Enums;
using MaintenanceModeMiddleware.Services;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MaintenanceModeMiddleware.Configuration.Options
{
    internal class DefaultResponseOption : Option<bool>, IRequestHandler
    {
        private const int DEFAULT_503_RETRY_INTERVAL = 5300;
        private readonly IDirectoryMapperService _dirMapperSvc;

        public override void LoadFromString(string str)
        {
            Value = bool.Parse(str);
        }

        public override string GetStringValue()
        {
            return Value.ToString();
        }

        private MaintenanceResponse GetResponse()
        {
            using (Stream resStream = GetType()
                    .Assembly
                    .GetManifestResourceStream($"{nameof(MaintenanceModeMiddleware)}.Resources.DefaultResponse.html"))
            {
                using var resSr = new StreamReader(resStream, Encoding.UTF8);
                return new MaintenanceResponse
                {
                    ContentBytes = resSr.CurrentEncoding.GetBytes(resSr.ReadToEnd()),
                    ContentEncoding = resSr.CurrentEncoding,
                    ContentType = ResponseContentType.Html,
                    Code503RetryInterval = DEFAULT_503_RETRY_INTERVAL
                };
            }
        }


        Task IRequestHandler.Postprocess(HttpContext context)
        {
            return Task.CompletedTask;
        }

        async Task<PreprocessResult> IRequestHandler.Preprocess(HttpContext context)
        {
            MaintenanceResponse response = GetResponse();

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
    }
}
