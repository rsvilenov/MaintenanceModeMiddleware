using MaintenanceModeMiddleware.Configuration.Data;
using Microsoft.AspNetCore.Http;

namespace MaintenanceModeMiddleware.Configuration.Builders
{
    internal class PathRedirectOptionsBulder : StatusCodeOptionsBulder, IPathRedirectOptionsBulder
    {
        private readonly ReturnUrlData _data;

        public PathRedirectOptionsBulder()
        {
            _data = new ReturnUrlData();
        }
        public void PassReturnPathAsParameter(string parameterName = "maintenanceReturnPath")
        {
            _data.SetReturnUrlInUrlParameter = true;
            _data.ReturnUrlUrlParameterName = parameterName;
        }

        public void SetReturnPathInCookie(string cookiePrefix = "maintenanceReturnPath", CookieOptions cookieOptions = null)
        {
            _data.SetReturnUrlInCookie = true;
            _data.ReturnUrlCookiePrefix = cookiePrefix;
            _data.ReturnUrlCookieOptions = cookieOptions;
        }

        public void SetCustomReturnPath(PathString returnPath)
        {
            _data.CustomReturnPath = returnPath;
        }

        public ReturnUrlData GetReturnUrlData()
        {
            return _data;
        }
    }
}
