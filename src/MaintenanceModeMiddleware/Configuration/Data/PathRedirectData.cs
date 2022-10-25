using Microsoft.AspNetCore.Http;

namespace MaintenanceModeMiddleware.Configuration.Data
{
    internal class PathRedirectData
    {
        public PathString Path { get; set; }
        public ResponseStatusCodeData StatusCodeData { get; set; }
    }
}
