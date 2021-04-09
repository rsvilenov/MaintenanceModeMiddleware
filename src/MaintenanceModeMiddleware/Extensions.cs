using MaintenanceModeMiddleware.Configuration.Builders;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace MaintenanceModeMiddleware
{
    public static class Extensions
    {
        public static IServiceCollection AddMaintenance(this IServiceCollection services, Action<ServiceOptionsBuilder> optionBuilderDelegate = null)
        {
            services.AddSingleton<IMaintenanceControlService>(svcProvider =>
                new MaintenanceControlService(svcProvider, optionBuilderDelegate));

            return services;
        }

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
