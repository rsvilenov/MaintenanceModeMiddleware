using MaintenanceModeMiddleware.Configuration.Data;
using Microsoft.AspNetCore.Hosting;

namespace MaintenanceModeMiddleware.Configuration
{
    internal interface IResponseHolder : IOption
    {
        MaintenanceResponse GetResponse(IWebHostEnvironment webHostEnv);
    }
}
