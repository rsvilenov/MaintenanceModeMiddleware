using Microsoft.AspNetCore.Hosting;

namespace MaintenanceModeMiddleware.Configuration
{
    internal interface IOption
    {
        void Verify(IWebHostEnvironment webHostEnv);
    }
}
