using System;

namespace MaintenanceModeMiddleware.Configuration.State
{
    public class MaintenanceState
    {
        internal MaintenanceState() { }
        public DateTime? ExpirationDate { get; internal set; }
        public bool IsMaintenanceOn { get; internal set; }
        internal OptionCollection MiddlewareOptions { get; set; }
    }
}
