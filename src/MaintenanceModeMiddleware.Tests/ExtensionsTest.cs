using MaintenanceModeMiddleware.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using System;
using Xunit;

namespace MaintenanceModeMiddleware.Tests
{
    public class ExtensionsTest
    {
        [Fact]
        public void IApplicationBuilder_UseMaintenance_NoService()
        {
            var appBuilder = Substitute.For<IApplicationBuilder>();
            appBuilder.ApplicationServices
                .Returns(Substitute.For<IServiceProvider>());
            Action testAction = () => appBuilder.UseMaintenance();

            testAction.ShouldThrow<InvalidOperationException>()
                .Message.ShouldStartWith("Unable to find the required service.");
        }

        [Fact]
        public void IApplicationBuilder_UseMaintenance()
        {
            // TODO: check the parameters and that the registration actually happened

            ServiceCollection svcCollection = new ServiceCollection();
            svcCollection.AddMaintenance();
            ServiceProvider serviceProvider = svcCollection.BuildServiceProvider();
            ApplicationBuilder appBuilder = new ApplicationBuilder(serviceProvider);
            Action testAction = () => appBuilder.UseMaintenance();

            testAction.ShouldNotThrow();
        }

        [Fact]
        public void IServiceCollection_AddMaintenance()
        {
            var svcCollection = new ServiceCollection();

            svcCollection.AddMaintenance();

            svcCollection.ShouldNotBeEmpty();
        }
    }
}
