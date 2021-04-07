using System;
using System.Collections.Generic;

namespace MaintenanceModeMiddleware.Configuration.State
{
    [Serializable]
    public class MaintenanceState
    {
        public DateTime? EndsOn { get; set; }
        public bool IsMaintenanceOn { get; set; }
        public List<StorableOption> MiddlewareOptions { get; set; }
    }
}
