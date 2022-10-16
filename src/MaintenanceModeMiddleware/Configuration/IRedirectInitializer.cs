using Microsoft.AspNetCore.Http;

namespace MaintenanceModeMiddleware.Configuration
{
    internal interface IRedirectInitializer : IOption
    {
        PathString RedirectPath { get; }
    }
}
