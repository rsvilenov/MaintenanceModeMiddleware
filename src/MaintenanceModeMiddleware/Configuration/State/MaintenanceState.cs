using System;
using System.Diagnostics;

namespace MaintenanceModeMiddleware.Configuration.State
{
    public class MaintenanceState
    {
        internal MaintenanceState() { }

        /// <summary>
        /// Gets the date and time when the maintenance mode will
        /// automatically be switched off.
        /// </summary>
        public DateTime? ExpirationDate { get; internal set; }

        /// <summary>
        /// Checks whether the maintenance mode is on.
        /// </summary>
        public bool IsMaintenanceOn { get; internal set; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal OptionCollection MiddlewareOptions { get; set; }
    }
}
