using MaintenanceModeMiddleware.Configuration.Enums;

namespace MaintenanceModeMiddleware.Services
{
    internal interface IDirectoryMapperService
    {
        string GetAbsolutePath(EnvDirectory dir);
    }
}
