using MaintenanceModeMiddleware.Configuration.Builders;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace MaintenanceModeMiddleware
{
    public static class Extensions
    {
        /// <summary>
        /// Adds the maintenance control service in the DI container.
        /// </summary>
        /// <param name="optionBuilderDelegate">Optional configuraiton of the service.</param>
        /// <returns></returns>
        public static IServiceCollection AddMaintenance(this IServiceCollection services, Action<ServiceOptionsBuilder> optionBuilderDelegate = null)
        {
            services.AddSingleton<IMaintenanceControlService>(svcProvider =>
                new MaintenanceControlService(svcProvider, 
                    svcProvider.GetService<IWebHostEnvironment>(),
                    optionBuilderDelegate));

            return services;
        }

        /// <summary>
        /// Adds the maintenance middleware to the middleware chain.
        /// </summary>
        /// <param name="options">Optional configuration.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseMaintenance(this IApplicationBuilder builder, Action<MiddlewareOptionsBuilder> options = null)
        {
            var maintenanceSvc = builder.ApplicationServices.GetService<IMaintenanceControlService>();
            if (maintenanceSvc == null)
            {
                throw new InvalidOperationException($"Unable to find the required service. You should call {nameof(AddMaintenance)} in Startup's Configure method.");
            }

            return builder.UseMiddleware<MaintenanceMiddleware>(options);
        }
    }
}
