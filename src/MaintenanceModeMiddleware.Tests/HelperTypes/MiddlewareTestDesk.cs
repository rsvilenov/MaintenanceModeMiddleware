using Microsoft.AspNetCore.Http;

namespace MaintenanceModeMiddleware.Tests.HelperTypes
{
    public class MiddlewareTestDesk
    {
        public MaintenanceMiddleware MiddlewareInstance { get; set; }
        public HttpContext CurrentHttpContext { get; set; }
        public bool IsNextDelegateCalled { get; set; }
    }
}
