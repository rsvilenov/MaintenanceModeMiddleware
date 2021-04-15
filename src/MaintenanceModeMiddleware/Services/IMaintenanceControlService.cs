using MaintenanceModeMiddleware.Configuration.Builders;
using MaintenanceModeMiddleware.Configuration.State;
using System;

namespace MaintenanceModeMiddleware.Services
{
    /// <summary>
    /// An interface for the maintenance control service.
    /// </summary>
    public interface IMaintenanceControlService
    {
        /// <summary>
        /// Puts the web application in maintenance mode.
        /// </summary>
        /// <param name="endsOn">Optional: specify the date when the maintenance mode will automatically end</param>
        /// <param name="middlewareOptions">Optional: specify middleware options.</param>
        void EnterMaintanence(DateTime? endsOn = null,
            Action<MiddlewareOptionsBuilder> middlewareOptions = null);

        /// <summary>
        /// Puts the web application back in normal (non-maintenance) mode.
        /// </summary>
        void LeaveMaintanence();

        /// <summary>
        /// Gets the current maintenance state.
        /// </summary>
        /// <returns></returns>
        MaintenanceState GetState();
    }
}
