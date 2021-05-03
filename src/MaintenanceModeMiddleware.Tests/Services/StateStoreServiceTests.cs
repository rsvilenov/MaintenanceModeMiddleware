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
        public void Constructor_WithStateStoreParam_ShouldNotThrow()
        {
            Action testAction = () => new StateStoreService(new InMemoryStateStore());

            testAction.ShouldNotThrow();
        }

        [Fact]
        public void SetState_WithValidState_ShouldNotThrow()
        {
            IStateStoreService stateStoreSvc = new StateStoreService(new InMemoryStateStore());
            IDirectoryMapperService dirMapperSvc = FakeDirectoryMapperService.Create();
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(dirMapperSvc);
            builder.BypassAllAuthenticatedUsers();

            Action testAction = () => stateStoreSvc.SetState(new MaintenanceState(null, isMaintenanceOn: true, builder.GetOptions()));

            testAction.ShouldNotThrow();
        }

        [Fact]
        public void GetState_AfterStoredState_ShouldRestoreSameState()
        {
            // store
            IStateStoreService stateStoreSvc1 = new StateStoreService(new InMemoryStateStore());
            IDirectoryMapperService dirMapperSvc = FakeDirectoryMapperService.Create();
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(dirMapperSvc);
            builder.BypassAllAuthenticatedUsers();

            stateStoreSvc1.SetState(new MaintenanceState(null, isMaintenanceOn: true, builder.GetOptions()));

            // restore
            IStateStoreService stateStoreSvc2 = new StateStoreService(new InMemoryStateStore());

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
            IStateStoreService svc = new StateStoreService(new InMemoryStateStore());

            Action testAction = () => svc.SetState(new MaintenanceState
            {
                IsMaintenanceOn = true
            });

            testAction.ShouldNotThrow();
        }

        [Fact]
        public void SetState_WhenStateStoreIsNotSet_ShouldNotThrow()
        {
            IStateStoreService svc = new StateStoreService(null);

            Action testAction = () => svc.SetState(new MaintenanceState
            {
                IsMaintenanceOn = true
            });

            testAction.ShouldNotThrow();
        }

        [Fact]
        public void GetState_WhenStateStoreIsNotSet_ShouldNotThrow()
        {
            IStateStoreService svc = new StateStoreService(null);

            Action testAction = () => svc.GetState();

            testAction.ShouldNotThrow();
        }

        [Fact]
        public void RestoreState_WhenStateStoreReturnsNull_ShouldNotReturnNull()
        {
            IStateStore stateStore = Substitute.For<IStateStore>();
            IStateStoreService svc = new StateStoreService(stateStore);
            stateStore.GetState().ReturnsNull();

            MaintenanceState state = svc.GetState();

            state.ShouldNotBeNull();
        }
    }
}
