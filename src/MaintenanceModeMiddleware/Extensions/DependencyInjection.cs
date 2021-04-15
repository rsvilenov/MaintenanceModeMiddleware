using MaintenanceModeMiddleware.Configuration.Builders;
using MaintenanceModeMiddleware.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace MaintenanceModeMiddleware.Extensions
{
    public static class DependencyInjection
    {
        /// <summary>
        /// Adds the maintenance control service in the DI container.
        /// </summary>
        /// <param name="optionBuilderDelegate">Optional configuraiton of the service.</param>
        /// <returns></returns>
        public static IServiceCollection AddMaintenance(this IServiceCollection services,
            Action<ServiceOptionsBuilder> optionBuilderDelegate = null)
        {
            services.AddSingleton<IStateStoreService, StateStoreService>();

            services.AddSingleton<IMaintenanceControlService>(svcProvider =>
                new MaintenanceControlService(
                    svcProvider.GetService<IWebHostEnvironment>(),
                    svcProvider.GetService<IStateStoreService>(),
                    optionBuilderDelegate));

            return services;
        }
    }
}
