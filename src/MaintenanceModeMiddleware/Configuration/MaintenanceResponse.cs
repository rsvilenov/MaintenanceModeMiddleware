using System;
using System.Text;

namespace MaintenanceModeMiddleware.Configuration
{
    internal class MaintenanceResponse
    {
        public ContentType ContentType { get; set; }
        public Encoding ContentEncoding { get; set; }
        public byte[] ContentBytes { get; set; }

        public string GetContentTypeString()
        {
            return ContentType switch
            {
                ContentType.Html => "text/html",
                ContentType.Text => "text/plain",
                _ => throw new InvalidOperationException("Content type could not be translated."),
            };
        }
    }
}
