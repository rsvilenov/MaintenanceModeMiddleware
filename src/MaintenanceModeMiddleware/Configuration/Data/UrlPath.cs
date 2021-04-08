using Microsoft.AspNetCore.Http;
using System;

namespace MaintenanceModeMiddleware.Configuration.Data
{
    internal class UrlPath
    {
        internal PathString PathString { get; set; }
        internal StringComparison Comparison { get; set; }
    }
}
