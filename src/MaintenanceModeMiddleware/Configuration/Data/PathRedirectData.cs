using Microsoft.AspNetCore.Http;

namespace MaintenanceModeMiddleware.Configuration.Data
{
    internal class PathRedirectData
    {
        public PathString Path { get; set; }
        public int Code503RetryInterval { get; set; }
        public bool Set503ResponseCode { get; set; }
    }
}
