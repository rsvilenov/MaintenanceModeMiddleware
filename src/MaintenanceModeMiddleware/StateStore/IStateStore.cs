using MaintenanceModeMiddleware.Configuration.State;

namespace MaintenanceModeMiddleware.StateStore
{
    public interface IStateStore
    {
        MaintenanceState GetState();
        void SetState(MaintenanceState state);
    }
}
