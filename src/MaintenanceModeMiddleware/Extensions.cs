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
            return builder.UseMiddleware<MaintenanceMiddleware>(options);
        }
    }
}
