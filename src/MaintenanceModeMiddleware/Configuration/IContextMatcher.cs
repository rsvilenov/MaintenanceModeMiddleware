using Microsoft.AspNetCore.Http;

namespace MaintenanceModeMiddleware.Configuration
{
    interface IContextMatcher : IOption
    {
        bool IsMatch(HttpContext context);
    }
}
