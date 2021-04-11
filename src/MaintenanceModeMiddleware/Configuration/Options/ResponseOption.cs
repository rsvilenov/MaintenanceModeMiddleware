using MaintenanceModeMiddleware.Configuration.Data;
using MaintenanceModeMiddleware.Configuration.Enums;
using System;
using System.Text;

namespace MaintenanceModeMiddleware.Configuration.Options
{
    internal class ResponseOption : Option<MaintenanceResponse>
    {
        internal const char PARTS_SEPARATOR = ';';

        public override void LoadFromString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentNullException(nameof(str));
            }

            string[] parts = str.Split(PARTS_SEPARATOR);
            if (parts.Length != 3)
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

            Encoding encoding = Encoding.GetEncoding(codePage);
            byte[] contentBytes = encoding.GetBytes(parts[2]);

            Value = new MaintenanceResponse
            {
                ContentBytes = contentBytes,
                ContentEncoding = encoding,
                ContentType = contentType
            };
        }

        public override string GetStringValue()
        {
            return $"{Value.ContentType};{Value.ContentEncoding.CodePage};{Value.ContentEncoding.GetString(Value.ContentBytes)}";
        }
    }
}
