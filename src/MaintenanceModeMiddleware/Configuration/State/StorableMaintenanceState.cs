using System;
using System.Collections.Generic;

namespace MaintenanceModeMiddleware.Configuration.State
{
    [Serializable]
    public class StorableMaintenanceState
    {
        public DateTime? ExpirationDate { get; set; }
        public bool IsMaintenanceOn { get; set; }
        public List<StorableOption> MiddlewareOptions { get; set; }
    }
}
