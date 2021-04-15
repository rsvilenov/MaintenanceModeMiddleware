using Microsoft.AspNetCore.Http;

namespace MaintenanceModeMiddleware.Configuration
{
    interface IAllowedRequestMatcher : IOption
    {
        bool IsMatch(HttpContext context);
    }
}
