using MaintenanceModeMiddleware.Configuration.Builders;
using MaintenanceModeMiddleware.Configuration.Options;
using MaintenanceModeMiddleware.Configuration.State;
using MaintenanceModeMiddleware.Services;
using MaintenanceModeMiddleware.StateStore;
using MaintenanceModeMiddleware.Tests.HelperTypes;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Shouldly;
using System;
using Xunit;

namespace MaintenanceModeMiddleware.Tests.Services
{
    public class StateStoreServiceTests
    {
        private readonly IServiceProvider _svcProvider = Substitute.For<IServiceProvider>();

        [Fact]
        public void SetStateStore_WithStoreParam_ShouldNotThrow()
        {
            IStateStoreService svc = new StateStoreService(_svcProvider);
            svc.SetStateStore(new InMemoryStateStore());

            Action testAction = () => svc.SetStateStore(new InMemoryStateStore());

            testAction.ShouldNotThrow();
        }

        [Fact]
        public void SetState_WithValidState_ShouldNotThrow()
        {
            IStateStoreService stateStoreSvc = new StateStoreService(_svcProvider);
            stateStoreSvc.SetStateStore(new InMemoryStateStore());
            IPathMapperService pathMapperSvc = FakePathMapperService.Create();
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(pathMapperSvc);
            builder.BypassAllAuthenticatedUsers();

            Action testAction = () => stateStoreSvc.SetState(new MaintenanceState(null, isMaintenanceOn: true, builder.GetOptions()));

            testAction.ShouldNotThrow();
        }

        [Fact]
        public void GetState_AfterStoredState_ShouldRestoreSameState()
        {
            // store
            IStateStoreService stateStoreSvc1 = new StateStoreService(_svcProvider);
            stateStoreSvc1.SetStateStore(new InMemoryStateStore());
            IPathMapperService pathMapperSvc = FakePathMapperService.Create();
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(pathMapperSvc);
            builder.BypassAllAuthenticatedUsers();

            stateStoreSvc1.SetState(new MaintenanceState(null, isMaintenanceOn: true, builder.GetOptions()));

            // restore
            IStateStoreService stateStoreSvc2 = new StateStoreService(_svcProvider);
            stateStoreSvc2.SetStateStore(new InMemoryStateStore());

            Func<MaintenanceState> testFunc = () => stateStoreSvc2.GetState();

            MaintenanceState state = testFunc.ShouldNotThrow()
                .ShouldNotBeNull();
            IMiddlewareOptionsContainer optionsContainer = state;

            optionsContainer.MiddlewareOptions
                .ShouldNotBeNull()
                .Any<BypassAllAuthenticatedUsersOption>()
                .ShouldBeTrue();
        }

        [Fact]
        public void SetState_WithNoMaintenanceOptions_ShouldNotThrow()
        {
            IStateStoreService svc = new StateStoreService(_svcProvider);
            svc.SetStateStore(new InMemoryStateStore());

            Action testAction = () => svc.SetState(new MaintenanceState
            {
                IsMaintenanceOn = true
            });

            testAction.ShouldNotThrow();
        }

        [Fact]
        public void SetStateStore_WithSvcConsumer_ShouldCallServiceProvider()
        {
            IStateStoreService svc = new StateStoreService(_svcProvider);
            IStateStore svcConsumer = Substitute.For<IStateStore, IServiceConsumer>();

            svc.SetStateStore(svcConsumer);

            ((IServiceConsumer)svcConsumer).ServiceProvider
                .Received(1);
        }

        [Fact]
        public void RestoreState_WhenStateStoreIsNotSet_ShouldNotThrow()
        {
            IStateStoreService svc = new StateStoreService(_svcProvider);
            IStateStore stateStore = Substitute.For<IStateStore>();

            Action testAction = () => svc.SetStateStore(stateStore);

            testAction.ShouldNotThrow();
        }

        [Fact]
        public void RestoreState_WhenStateStoreReturnsNull_ShouldNotReturnNull()
        {
            IStateStoreService svc = new StateStoreService(_svcProvider);
            IStateStore stateStore = Substitute.For<IStateStore>();
            stateStore.GetState().ReturnsNull();
            svc.SetStateStore(stateStore);

            MaintenanceState state = svc.GetState();

            state.ShouldNotBeNull();
        }
    }
}
