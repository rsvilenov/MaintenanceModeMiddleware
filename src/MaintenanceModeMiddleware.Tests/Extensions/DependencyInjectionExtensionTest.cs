using MaintenanceModeMiddleware.Extensions;
using MaintenanceModeMiddleware.Services;
using MaintenanceModeMiddleware.StateStore;
using MaintenanceModeMiddleware.Tests.HelperTypes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using Xunit;

namespace MaintenanceModeMiddleware.Tests.Extensions
{
    public class DependencyInjectionExtensionTest
    {
        [Fact]
        public void AddMaintenance_WhenUsed_ShouldAddServices()
        {
            var svcCollection = new ServiceCollection();
            svcCollection.AddSingleton(Substitute.For<IWebHostEnvironment>());

            svcCollection.AddMaintenance();

            svcCollection
                .BuildServiceProvider()
                .GetService<IMaintenanceControlService>()
                .ShouldNotBeNull();
        }

        [Fact]
        public void AddMaintenance_WithOptions_OptionBuilderDelegateShouldBeCalled()
        {
            var svcCollection = new ServiceCollection();
            bool delegateCalled = false;

            svcCollection.AddMaintenance((options) => delegateCalled = true);

            delegateCalled.ShouldBeTrue();
        }

        [Fact]
        public void AddMaintenance_WithCustomStateSTore_StoreShouldBeRegistered()
        {
            var svcCollection = new ServiceCollection();
            svcCollection.AddSingleton(Substitute.For<IWebHostEnvironment>());

            svcCollection.AddMaintenance((options) => options.UseStateStore<InMemoryStateStore>());
            
            svcCollection
                .BuildServiceProvider()
                .GetService<IStateStore>()
                .ShouldBeOfType<InMemoryStateStore>();
        }

        [Fact]
        public void AddMaintenance_WithCustomStateSToreObject_StoreShouldBeRegistered()
        {
            var svcCollection = new ServiceCollection();
            svcCollection.AddSingleton(Substitute.For<IWebHostEnvironment>());
            var stateStore = new InMemoryStateStore();

            svcCollection.AddMaintenance((options) => options.UseStateStore(stateStore));

            svcCollection
                .BuildServiceProvider()
                .GetService<IStateStore>()
                .ShouldBeOfType<InMemoryStateStore>();
        }
    }
}
