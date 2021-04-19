using Microsoft.AspNetCore.Http;

namespace MaintenanceModeMiddleware.Configuration
{
    internal interface IAllowedRequestMatcher : IOption
    {
        bool IsMatch(HttpContext context);
    }
}
