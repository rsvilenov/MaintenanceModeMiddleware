using MaintenanceModeMiddleware.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using System;
using System.Collections.Generic;
using Xunit;
using MaintenanceModeMiddleware;
using System.Linq;
using Microsoft.AspNetCore.Builder;

namespace MaintenanceModeMiddleware.Tests
{
    public class Extensions
    {
        [Fact]
        public void IServiceCollection_AddMaintenance()
        {
            var svcCollection = new ServiceCollection();

            svcCollection.AddMaintenance();

            svcCollection.ShouldNotBeEmpty();
        }

        [Fact]
        public void IApplicationBuilder_UseMaintenance()
        {
            // TODO: check the parameters and that the registration actually happened
            var appBuilder = Substitute.For<IApplicationBuilder>();
            appBuilder.ApplicationServices
                .Returns(Substitute.For<IServiceProvider>());
            Action testAction = () => appBuilder.UseMaintenance();

            testAction.ShouldNotThrow();
        }
    }
}
