using MaintenanceModeMiddleware.Configuration;
using MaintenanceModeMiddleware.StateStore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace MaintenanceModeMiddleware
{
    public static class Extensions
    {
        public static IServiceCollection AddMaintenance(this IServiceCollection services, IStateStore stateStore = null)
        {
            services.AddSingleton<IMaintenanceControlService>(svcProvider =>
                new MaintenanceControlService(svcProvider, stateStore));

            return services;
        }

        public static IApplicationBuilder UseMaintenance(this IApplicationBuilder builder, Action<Options> options = null)
        {
            return builder.UseMiddleware<MaintenanceMiddleware>(options);
        }
    }
}
