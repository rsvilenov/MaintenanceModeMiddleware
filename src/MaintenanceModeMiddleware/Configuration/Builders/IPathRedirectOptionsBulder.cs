using MaintenanceModeMiddleware.Configuration.Data;
using Microsoft.AspNetCore.Http;

namespace MaintenanceModeMiddleware.Configuration.Builders
{
    public interface IPathRedirectOptionsBulder : IStatusCodeOptionsBuilder
    {
        void PassReturnPathAsParameter(string parameterName = "maintenanceReturnPath");
        void SetReturnPathInCookie(string cookiePrefix = "maintenanceReturnPath", CookieOptions cookieOptions = null);
        void SetCustomReturnPath(PathString returnPath);
    }
}
