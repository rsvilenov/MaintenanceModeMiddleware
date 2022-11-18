using MaintenanceModeMiddleware.Configuration.Data;
using Microsoft.AspNetCore.Http;

namespace MaintenanceModeMiddleware.Configuration.Builders
{
    public class PathRedirectOptionsBuilder<TBuilder> : 
        StatusCodeOptionsBuilder<TBuilder>
        where TBuilder : PathRedirectOptionsBuilder<TBuilder>
    {
        private readonly ReturnUrlData _data;

        internal PathRedirectOptionsBuilder()
        {
            _data = new ReturnUrlData();
        }

        public TBuilder PassReturnPathAsParameter(string parameterName = "maintenanceReturnPath")
        {
            _data.SetReturnUrlInUrlParameter = true;
            _data.ReturnUrlParameterName = parameterName;
            return (TBuilder)this;
        }

        public TBuilder SetReturnPathInCookie(string cookiePrefix = "maintenanceReturnPath", CookieOptions cookieOptions = null)
        {
            _data.SetReturnUrlInCookie = true;
            _data.ReturnUrlCookiePrefix = cookiePrefix;
            _data.ReturnUrlCookieOptions = cookieOptions;
            return (TBuilder)this;
        }

        public TBuilder SetCustomReturnPath(PathString returnPath)
        {
            _data.CustomReturnPath = returnPath;
            return (TBuilder)this;
        }

        internal ReturnUrlData GetReturnUrlData()
        {
            return _data;
        }
    }

    public class PathRedirectOptionsBuilder : PathRedirectOptionsBuilder<PathRedirectOptionsBuilder>
    {
        internal PathRedirectOptionsBuilder()
        {
        }
    }
}
