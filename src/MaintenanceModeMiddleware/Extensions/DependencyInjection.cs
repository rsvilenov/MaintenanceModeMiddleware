using MaintenanceModeMiddleware.Configuration.Builders;
using MaintenanceModeMiddleware.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace MaintenanceModeMiddleware.Extensions
{
    public static class DependencyInjection
    {
        /// <summary>
        /// Adds the maintenance control service and its dependencies to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="optionBuilderDelegate">Optional configuraiton for the service.</param>
        /// <returns>The same instance of the <see cref="IServiceCollection"/> for chaining.</returns>
        public static IServiceCollection AddMaintenance(this IServiceCollection services,
            Action<IServiceOptionsBuilder> options = null)
        {
            services.AddSingleton<IPathMapperService, PathMapperService>();

            services.AddSingleton<IStateStoreService, StateStoreService>();

            services.AddSingleton<IMaintenanceControlService>(svcProvider =>
                new MaintenanceControlService(
                    svcProvider.GetService<IPathMapperService>(),
                    svcProvider.GetService<IStateStoreService>(),
                    options));

            return services;
        }
    }
}
