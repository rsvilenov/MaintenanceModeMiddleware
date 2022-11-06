using MaintenanceModeMiddleware.Configuration.Builders;
using MaintenanceModeMiddleware.Services;
using Microsoft.AspNetCore.Builder;
using System;

namespace MaintenanceModeMiddleware.Extensions
{
    public static class Configuration
    {
        /// <summary>
        /// Adds the maintenance middleware to the middleware chain.
        /// </summary>
        /// <param name="options">Optional configuration.</param>
        /// <returns>The same <see cref="IApplicationBuilder"/> instance so that multiple calls can be chained.</returns>
        public static IApplicationBuilder UseMaintenance(this IApplicationBuilder builder, 
            Action<IMiddlewareOptionsBuilder> options = null)
        {
            var maintenanceSvc = builder.ApplicationServices
                .GetService(typeof(IMaintenanceControlService));
            if (maintenanceSvc == null)
            {
                throw new InvalidOperationException($"Unable to find {nameof(IMaintenanceControlService)}. You should call {nameof(DependencyInjection.AddMaintenance)} in Startup's Configure method.");
            }

            IDirectoryMapperService dirMapperSvc = builder.ApplicationServices
                .GetService(typeof(IDirectoryMapperService)) as IDirectoryMapperService;
            if (maintenanceSvc == null)
            {
                throw new InvalidOperationException($"Unable to find {nameof(IDirectoryMapperService)}. You should call {nameof(DependencyInjection.AddMaintenance)} in Startup's Configure method.");
            }

            IMaintenanceOptionsService optionsService = builder.ApplicationServices
               .GetService(typeof(IMaintenanceOptionsService)) as IMaintenanceOptionsService;
            if (maintenanceSvc == null)
            {
                throw new InvalidOperationException($"Unable to find {nameof(IMaintenanceOptionsService)}. You should call {nameof(DependencyInjection.AddMaintenance)} in Startup's Configure method.");
            }

            var optionsBuilder = new MiddlewareOptionsBuilder(dirMapperSvc);
            options?.Invoke(optionsBuilder);

            optionsService.SetStartupOptions(optionsBuilder.GetOptions());

            return builder.UseMiddleware<MaintenanceMiddleware>();
        }
    }
}
