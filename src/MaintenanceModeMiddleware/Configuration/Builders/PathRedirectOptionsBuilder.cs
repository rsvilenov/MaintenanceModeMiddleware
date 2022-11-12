using MaintenanceModeMiddleware.Configuration.Data;
using Microsoft.AspNetCore.Http;

namespace MaintenanceModeMiddleware.Configuration.Builders
{
    internal class PathRedirectOptionsBuilder : StatusCodeOptionsBuilder, IPathRedirectOptionsBuilder
    {
        private readonly ReturnUrlData _data;

        public PathRedirectOptionsBuilder()
        {
            _data = new ReturnUrlData();
        }

        public void PassReturnPathAsParameter(string parameterName = "maintenanceReturnPath")
        {
            _data.SetReturnUrlInUrlParameter = true;
            _data.ReturnUrlParameterName = parameterName;
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
