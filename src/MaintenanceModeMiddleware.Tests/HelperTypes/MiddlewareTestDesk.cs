using Microsoft.AspNetCore.Http;

namespace MaintenanceModeMiddleware.Tests.HelperTypes
{
    internal class MiddlewareTestDesk
    {
        internal MaintenanceMiddleware MiddlewareInstance { get; set; }
        internal HttpContext CurrentHttpContext { get; set; }
        internal bool IsNextDelegateCalled { get; set; }
    }
}
