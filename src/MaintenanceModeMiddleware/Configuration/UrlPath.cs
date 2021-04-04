using Microsoft.AspNetCore.Http;
using System;

namespace MaintenanceModeMiddleware.Configuration
{
    internal class UrlPath
    {
        internal PathString String { get; set; }
        internal StringComparison Comparison { get; set; }
    }
}
