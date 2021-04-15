using MaintenanceModeMiddleware.Configuration.Data;
using MaintenanceModeMiddleware.Configuration.Enums;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Text;

namespace MaintenanceModeMiddleware.Configuration.Options
{
    internal class ResponseOption : Option<MaintenanceResponse>, IResponseHolder
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
            
            if (!Enum.TryParse(parts[0], out ContentType contentType))
            {
                throw new ArgumentException($"Unknown content type {parts[0]}");
            }

            if (!int.TryParse(parts[1], out int codePage))
            {
                throw new FormatException($"The code page {parts[1]} is not a valid integer.");
            }

            if (!int.TryParse(parts[2], out int code503RetryInterval))
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

        public MaintenanceResponse GetResponse(IWebHostEnvironment webHostEnv)
        {
            return Value;
        }
    }
}
