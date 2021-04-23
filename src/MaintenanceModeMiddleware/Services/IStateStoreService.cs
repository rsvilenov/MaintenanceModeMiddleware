using MaintenanceModeMiddleware.Configuration.State;

namespace MaintenanceModeMiddleware.Services
{
    internal interface IStateStoreService
    {
        void SetState(MaintenanceState state);
        MaintenanceState GetState();
    }
}
