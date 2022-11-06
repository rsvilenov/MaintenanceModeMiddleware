using MaintenanceModeMiddleware.Configuration.Data;
using MaintenanceModeMiddleware.Configuration.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Text;
using System.Threading.Tasks;

namespace MaintenanceModeMiddleware.Configuration.Options
{
    internal class ResponseOption : Option<MaintenanceResponse>, IRequestHandler
    {
        private const char PARTS_SEPARATOR = ';';

        public override void LoadFromString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentNullException(nameof(str));
            }

            string[] parts = str.Split(PARTS_SEPARATOR);
            if (parts.Length != 4)
            {
                throw new FormatException($"{nameof(str)} is in incorrect format.");
            }
            
            if (!Enum.TryParse(parts[0], out ResponseContentType contentType))
            {
                throw new ArgumentException($"Unknown content type {parts[0]}");
            }

            if (!int.TryParse(parts[1], out int codePage))
            {
                throw new FormatException($"The code page {parts[1]} is not a valid integer.");
            }

            if (!uint.TryParse(parts[2], out uint code503RetryInterval))
            {
                throw new ArgumentException("Unable to parse the code 503 retry interval.");
            }

            Encoding encoding = Encoding.GetEncoding(codePage);
            byte[] contentBytes = encoding.GetBytes(parts[3]);

            Value = new MaintenanceResponse
            {
                ContentBytes = contentBytes,
                ContentEncoding = encoding,
                ContentType = contentType,
                Code503RetryInterval = code503RetryInterval
            };
        }

        public override string GetStringValue()
        {
            return $"{Value.ContentType}{PARTS_SEPARATOR}{Value.ContentEncoding.CodePage}{PARTS_SEPARATOR}{Value.Code503RetryInterval}{PARTS_SEPARATOR}{Value.ContentEncoding.GetString(Value.ContentBytes)}";
        }

        Task IRequestHandler.Postprocess(HttpContext context)
        {
            return Task.CompletedTask;
        }

        async Task<PreprocessResult> IRequestHandler.Preprocess(HttpContext context)
        {
            MaintenanceResponse response = Value;

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
