using MaintenanceModeMiddleware.Configuration;
using MaintenanceModeMiddleware.Configuration.Options;
using MaintenanceModeMiddleware.Extensions;
using MaintenanceModeMiddleware.Services;
using Microsoft.AspNetCore.Builder;
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
        public void UseMaintenance_WhenMaintenanceControlServiceIsNotRegistered_ShouldThrow()
        {
            var appBuilder = CreateApplicationBuilder(new Type[]
            {
                typeof(IDirectoryMapperService),
                typeof(IMaintenanceOptionsService)
            });

            Action testAction = () => appBuilder.UseMaintenance();

            testAction.ShouldThrow<InvalidOperationException>()
                .Message.ShouldStartWith("Unable to find IMaintenanceControlService. You should call AddMaintenance in Startup's Configure method.");
        }

        [Fact]
        public void UseMaintenance_WhenMaintenanceOptionsServiceIsNotRegistered_ShouldThrow()
        {
            var appBuilder = CreateApplicationBuilder(new Type[]
            {
                typeof(IMaintenanceControlService),
                typeof(IDirectoryMapperService)
            });

            Action testAction = () => appBuilder.UseMaintenance();

            testAction.ShouldThrow<InvalidOperationException>()
                .Message.ShouldStartWith("Unable to find IMaintenanceOptionsService. You should call AddMaintenance in Startup's Configure method.");
        }

        [Fact]
        public void UseMaintenance_WhenDirectoryMapperServiceIsNotRegistered_ShouldThrow()
        {
            var appBuilder = CreateApplicationBuilder(new Type[]
            {
                typeof(IMaintenanceControlService),
                typeof(IMaintenanceOptionsService)
            });

            Action testAction = () => appBuilder.UseMaintenance();

            testAction.ShouldThrow<InvalidOperationException>()
                .Message.ShouldStartWith("Unable to find IDirectoryMapperService. You should call AddMaintenance in Startup's Configure method.");
        }

        [Fact]
        public void UseMaintenance_WhenDependencyServicesAreRegistered_AppBuilderUseMethodShouldBeCalled()
        {
            var appBuilder = CreateApplicationBuilder(new Type[] 
            { 
                typeof(IMaintenanceControlService), 
                typeof(IDirectoryMapperService), 
                typeof(IMaintenanceOptionsService) 
            });

            appBuilder.UseMaintenance();

            appBuilder
                .Received(1)
                .Use(Arg.Any<Func<RequestDelegate, RequestDelegate>>());
        }

        [Fact]
        public void UseMaintenance_WithOptions_OptionsServiceShouldRecieveTheOptions()
        {
            IMaintenanceOptionsService maintenanceOptionsService = Substitute.For<IMaintenanceOptionsService>();

            var appBuilder = CreateApplicationBuilder(
                svcCollection => svcCollection.AddSingleton<IMaintenanceOptionsService>(maintenanceOptionsService),
                new Type[]
                {
                    typeof(IMaintenanceControlService),
                    typeof(IDirectoryMapperService)
                });

            appBuilder.UseMaintenance(options => options.UsePathRedirect("/test"));

            maintenanceOptionsService
                .Received(1)
                .SetStartupOptions(Arg.Is<OptionCollection>(collection => collection
                    .Any<PathRedirectOption>()));
        }

        private IApplicationBuilder CreateApplicationBuilder(params Type[] serviceTypes)
        {
            return CreateApplicationBuilder(null, serviceTypes);
        }

        private IApplicationBuilder CreateApplicationBuilder(Action<ServiceCollection> serviceCollectionModifyDelegate, params Type[] serviceTypes)
        {
            ServiceCollection svcCollection = new ServiceCollection();
            foreach (Type serviceType in serviceTypes)
            {
                svcCollection.AddSingleton(serviceType, Substitute.For(new Type[] { serviceType }, null));
            }

            serviceCollectionModifyDelegate?.Invoke(svcCollection);

            ServiceProvider serviceProvider = svcCollection.BuildServiceProvider();
            IApplicationBuilder appBuilder = Substitute.For<IApplicationBuilder>();
            appBuilder.ApplicationServices.Returns(serviceProvider);

            return appBuilder;
        }
    }
}
