using Microsoft.AspNetCore.Http;

namespace MaintenanceModeMiddleware.Configuration.Data
{
    internal class PathRedirectData
    {
        public PathString Path { get; set; }
        public uint Code503RetryInterval { get; set; }
        public bool Set503StatusCode { get; set; }
    }
}
