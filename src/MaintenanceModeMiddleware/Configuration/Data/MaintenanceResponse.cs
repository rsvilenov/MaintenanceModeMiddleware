using MaintenanceModeMiddleware.Configuration.Enums;
using System;
using System.Text;

namespace MaintenanceModeMiddleware.Configuration.Data
{
    internal class MaintenanceResponse
    {
        public ResponseContentType ContentType { get; set; }
        public Encoding ContentEncoding { get; set; }
        public byte[] ContentBytes { get; set; }
        public uint Code503RetryInterval { get; set; }
        public bool Set503StatusCode { get; } = true;

        public string GetContentTypeString()
        {
            return ContentType switch
            {
                ResponseContentType.Html => "text/html",
                ResponseContentType.Text => "text/plain",
                ResponseContentType.Json => "application/json",
                _ => throw new InvalidOperationException("Content type could not be translated."),
            };
        }
    }
}
