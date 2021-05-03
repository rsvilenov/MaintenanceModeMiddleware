using MaintenanceModeMiddleware.Configuration.Builders;
using MaintenanceModeMiddleware.Services;
using MaintenanceModeMiddleware.StateStore;
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
            Action<IStateStoreOptionsBuilder> options = null)
        {
            services.AddSingleton<IDirectoryMapperService, DirectoryMapperService>();

            services.RegisterStateStore(options);
            services.AddSingleton<IStateStoreService, StateStoreService>();

            services.AddSingleton<IMaintenanceControlService, MaintenanceControlService>();

            return services;
        }

        private static void RegisterStateStore(this IServiceCollection services, 
            Action<IStateStoreOptionsBuilder> options)
        {
            if (options == null)
            {
                services.AddSingleton<IStateStore, FileStateStore>();
                return;
            }
                
            StateStoreOptionsBuilder optionsBuilder = new StateStoreOptionsBuilder();
            options.Invoke(optionsBuilder);

            IStateStore instance = optionsBuilder.GetStateStoreInstance();
            if (instance != null)
            {
                services.AddSingleton(instance);
                return;
            }

            Type type = optionsBuilder.GetStateStoreType();
            if (type != null)
            {
                services.AddSingleton(typeof(IStateStore), type);
            }
        }
    }
}
