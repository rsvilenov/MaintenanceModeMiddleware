using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaintenanceModeMiddleware.Configuration
{
    public class Options
    {
        /// <summary>
        /// Response
        /// </summary>
        public MaintenanceResponse Response { get; set; }
        public File ResponseFile { get; set; }
        public int Code503RetryAfter { get; set; } = 5300;
        public List<string> BypassUserRoles { get; set; } = new List<string>()
        {
            "Admin"
        };

        public List<string> BypassUserNames { get; set; } = new List<string>();
        public bool BypassAuthenticatedUsers { get; set; } = false;
        public List<PathString> BypassUrlPaths { get; set; } = new List<PathString>
        {
            new PathString("/Identity")
        };

        public StringComparison PathStringComparison { get; set; } = StringComparison.OrdinalIgnoreCase;

        public List<string> BypassFileExtensions { get; set; } = new List<string>
        {
            "css"
        };
    }
}
