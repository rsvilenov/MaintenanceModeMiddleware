using MaintenanceModeMiddleware.Configuration;
using MaintenanceModeMiddleware.Configuration.Builders;
using System;

namespace MaintenanceModeMiddleware
{
    public interface IMaintenanceControlService
    {
        void EnterMaintanence(DateTime? endsOn = null,
            Action<MiddlewareOptionsBuilder> middlewareOptions = null);
        void LeaveMaintanence();
        bool IsMaintenanceModeOn { get; }
        public DateTime? EndsOn { get; }
    }

    internal interface ICanRestoreState
    {
        void RestoreState();
    }

    internal interface ICanOverrideMiddlewareOptions
    {
        OptionCollection GetOptionsToOverride();
    }
}
