using System;

namespace MaintenanceModeMiddleware
{
    public interface IMaintenanceControlService
    {
        void EnterMaintanence(DateTime? endsOn = null);
        void LeaveMaintanence();
        bool IsMaintenanceModeOn { get; }
    }

    internal interface ICanRestoreState
    {
        void RestoreState();
    }
}
