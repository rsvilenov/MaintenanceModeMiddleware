using MaintenanceModeMiddleware.Configuration.State;
using MaintenanceModeMiddleware.StateStore;

namespace MaintenanceModeMiddleware.Services
{
    internal interface IStateStoreService
    {
        void SetState(MaintenanceState state);
        MaintenanceState GetState();
    }
}
