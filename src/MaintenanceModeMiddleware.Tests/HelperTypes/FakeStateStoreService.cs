using MaintenanceModeMiddleware.Services;
using MaintenanceModeMiddleware.StateStore;
using NSubstitute;

namespace MaintenanceModeMiddleware.Tests.HelperTypes
{
    internal static class FakeStateStoreService
    {
        internal static IStateStoreService Create()
        {
            IStateStore stateStore = Substitute.For<IStateStore>();
            IStateStoreService stateStoreSvc = new StateStoreService(stateStore);
            return stateStoreSvc;
        }
    }
}
