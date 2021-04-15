using MaintenanceModeMiddleware.Services;
using NSubstitute;
using System;

namespace MaintenanceModeMiddleware.Tests.HelperTypes
{
    internal static class FakeStateStoreService
    {
        internal static IStateStoreService Create()
        {
            IServiceProvider svcProvider = Substitute.For<IServiceProvider>();
            IStateStoreService stateStoreSvc = new StateStoreService(svcProvider);
            stateStoreSvc.SetStateStore(new InMemoryStateStore());
            return stateStoreSvc;
        }
    }
}
