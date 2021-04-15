using MaintenanceModeMiddleware.Configuration;
using MaintenanceModeMiddleware.Configuration.Builders;
using System;

namespace MaintenanceModeMiddleware
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
        /// True if the web application is currently in maintenance mode.
        /// </summary>
        bool IsMaintenanceModeOn { get; }

        /// <summary>
        /// The date (if specified) when the maintenance on will end automatically.
        /// </summary>
        public DateTime? EndsOn { get; }
    }

    internal interface ICanRestoreState
    {
        /// <summary>
        /// Call me thwn the dependency graph is fully built.
        /// </summary>
        void RestoreState();
    }

    internal interface ICanOverrideMiddlewareOptions
    {
        OptionCollection GetOptionsToOverride();
    }
}
