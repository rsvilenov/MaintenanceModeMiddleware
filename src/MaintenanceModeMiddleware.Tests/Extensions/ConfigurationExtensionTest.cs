using MaintenanceModeMiddleware.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using System;
using Xunit;

namespace MaintenanceModeMiddleware.Tests.Extensions
{
    public class ConfigurationExtensionTest
    {
        [Fact]
        public void UseMaintenance_WhenServiceIsNotRegistered_ShouldThrow()
        {
            var appBuilder = Substitute.For<IApplicationBuilder>();
            appBuilder.ApplicationServices
                .Returns(Substitute.For<IServiceProvider>());
            Action testAction = () => appBuilder.UseMaintenance();

            testAction.ShouldThrow<InvalidOperationException>()
                .Message.ShouldStartWith("Unable to find the required service.");
        }

        [Fact]
        public void UseMaintenance_WhenServiceIsRegistered_AppBuilderUseMethodShouldBeCalled()
        {
            ServiceCollection svcCollection = new ServiceCollection();
            svcCollection.AddSingleton(Substitute.For<IWebHostEnvironment>());
            svcCollection.AddMaintenance();
            ServiceProvider serviceProvider = svcCollection.BuildServiceProvider();
            IApplicationBuilder appBuilder = Substitute.For<IApplicationBuilder>();
            appBuilder.ApplicationServices.Returns(serviceProvider);

            appBuilder.UseMaintenance();

            appBuilder.Use(Arg.Any<Func<RequestDelegate, RequestDelegate>>())
                .Received(1);
        }
    }
}
