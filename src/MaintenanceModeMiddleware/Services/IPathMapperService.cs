using MaintenanceModeMiddleware.Configuration.Enums;

namespace MaintenanceModeMiddleware.Services
{
    internal interface IPathMapperService
    {
        string GetPath(EnvDirectory dir);
    }
}
