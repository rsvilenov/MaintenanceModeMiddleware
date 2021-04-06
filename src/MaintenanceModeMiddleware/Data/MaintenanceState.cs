using MaintenanceModeMiddleware.Configuration;
using System;
using System.Collections.Generic;

namespace MaintenanceModeMiddleware.Data
{
    public class MaintenanceState
    {
        public DateTime? EndsOn { get; set; }
        public bool IsMaintenanceOn { get; set; }
        public List<StorableOption> MiddlewareOptions { get; set; }
    }
}
