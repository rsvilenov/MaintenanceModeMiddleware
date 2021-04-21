using System;

namespace MaintenanceModeMiddleware.Configuration.State
{
    public interface IMaintenanceState
    {
        /// <summary>
        /// Gets the date and time when the maintenance mode will
        /// automatically be switched off.
        /// </summary>
        DateTime? ExpirationDate { get; }

        /// <summary>
        /// Checks whether the maintenance mode is on.
        /// </summary>
        bool IsMaintenanceOn { get; }
    }
}
