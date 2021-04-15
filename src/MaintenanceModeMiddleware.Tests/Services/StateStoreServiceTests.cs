using MaintenanceModeMiddleware.Configuration.Builders;
using MaintenanceModeMiddleware.Configuration.Options;
using MaintenanceModeMiddleware.Configuration.State;
using MaintenanceModeMiddleware.Services;
using MaintenanceModeMiddleware.StateStore;
using MaintenanceModeMiddleware.Tests.HelperTypes;
using NSubstitute;
using Shouldly;
using System;
using Xunit;
using Xunit.Extensions.Ordering;

namespace MaintenanceModeMiddleware.Tests.Services
{
    [TestCaseOrderer("Xunit.Extensions.Ordering.TestCaseOrderer", "Xunit.Extensions.Ordering")]
    public class StateStoreServiceTests
    {
        private readonly IServiceProvider _svcProvider = Substitute.For<IServiceProvider>();

        [Fact]
        public void SetStateStore()
        {
            IStateStoreService svc = new StateStoreService(_svcProvider);
            svc.SetStateStore(new InMemoryStateStore());

            Action testAction = () => svc.SetStateStore(new InMemoryStateStore());

            testAction.ShouldNotThrow();
        }

        [Fact]
        [Order(1)]
        public void SetState()
        {
            IStateStoreService svc = new StateStoreService(_svcProvider);
            svc.SetStateStore(new InMemoryStateStore());
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(FakeWebHostEnvironment.Create());
            builder.BypassAllAuthenticatedUsers();

            Action testAction = () => svc.SetState(new MaintenanceState
            {
                IsMaintenanceOn = true,
                MiddlewareOptions = builder.GetOptions()
            });

            testAction.ShouldNotThrow();
        }

        [Fact]
        [Order(2)]
        public void RestoreState()
        {
            IStateStoreService svc = new StateStoreService(_svcProvider);
            svc.SetStateStore(new InMemoryStateStore());

            Func<MaintenanceState> testFunc = () => svc.GetState();

            testFunc.ShouldNotThrow()
                .ShouldNotBeNull()
                .MiddlewareOptions
                .ShouldNotBeNull()
                .Any<BypassAllAuthenticatedUsersOption>()
                .ShouldBeTrue();
        }

        [Fact]
        public void SetState_With_MaintenanceOptions_Null()
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
        public void RestoreState_When_StateStore_Is_SvcConsumer()
        {
            IStateStoreService svc = new StateStoreService(_svcProvider);
            IStateStore svcConsumer = Substitute.For<IStateStore, IServiceConsumer>();

            Action testAction = () => svc.SetStateStore(svcConsumer);

            testAction.ShouldNotThrow();
            ((IServiceConsumer)svcConsumer).ServiceProvider
                .Received(1);
        }

        [Fact]
        public void RestoreState_When_StateStore_Is_Not_Set_Should_Not_Throw()
        {
            IStateStoreService svc = new StateStoreService(_svcProvider);
            IStateStore svcConsumer = Substitute.For<IStateStore>();

            Action testAction = () => svc.SetStateStore(svcConsumer);

            testAction.ShouldNotThrow();
        }
    }
}
