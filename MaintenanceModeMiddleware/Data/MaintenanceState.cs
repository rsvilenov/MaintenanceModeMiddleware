using System;

namespace MaintenanceModeMiddleware.Data
{
    public class MaintenanceState
    {
        public DateTime? EndsOn { get; set; }
        public bool IsMaintenanceOn { get; set; }
    }
}
