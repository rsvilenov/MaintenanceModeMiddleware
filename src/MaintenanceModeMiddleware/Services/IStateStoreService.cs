using MaintenanceModeMiddleware.Configuration.State;
using MaintenanceModeMiddleware.StateStore;

namespace MaintenanceModeMiddleware.Services
{
    internal interface IStateStoreService
    {
        void SetStateStore(IStateStore stateStore);
        void SetState(MaintenanceState state);
        MaintenanceState GetState();
    }
}
