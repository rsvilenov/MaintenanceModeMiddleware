using System;
using System.Text;

namespace MaintenanceModeMiddleware.Configuration.Options
{
    internal class ResponseOption : Option<MaintenanceResponse>
    {
        private const char PARTS_SEPARATOR = ';';

        public override void FromString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentNullException(nameof(str));
            }

            string[] parts = str.Split(PARTS_SEPARATOR);
            if (parts.Length != 3)
            {
                throw new ArgumentException($"{nameof(str)} is in incorrect format.");
            }
            
            if (!Enum.TryParse(parts[0], out ContentType contentType))
            {
                throw new ArgumentException($"Unknown content type {parts[0]}");
            }

            Encoding encoding = Encoding.GetEncoding(parts[1]);
            byte[] contentBytes = encoding.GetBytes(parts[2]);

            Value = new MaintenanceResponse
            {
                ContentBytes = contentBytes,
                ContentEncoding = encoding,
                ContentType = contentType
            };
        }

        public override string ToString()
        {
            return $"{Value.ContentType};{Value.ContentEncoding};{Value.ContentEncoding.GetString(Value.ContentBytes)}";
        }
    }
}
