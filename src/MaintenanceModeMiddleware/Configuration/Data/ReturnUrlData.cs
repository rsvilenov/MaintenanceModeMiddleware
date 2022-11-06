using Microsoft.AspNetCore.Http;

namespace MaintenanceModeMiddleware.Configuration.Data
{
    internal class ReturnUrlData
    {
        public string ReturnUrlCookiePrefix { get; set; }
        public string ReturnUrlUrlParameterName { get; set; }
        public bool SetReturnUrlInCookie { get; set; }
        public bool SetReturnUrlInUrlParameter { get; set; }
        public CookieOptions ReturnUrlCookieOptions { get; set; }
        public PathString? CustomReturnPath { get; set; }
    }
}
