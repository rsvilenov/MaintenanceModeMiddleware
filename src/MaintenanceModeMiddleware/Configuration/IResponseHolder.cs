using MaintenanceModeMiddleware.Configuration.Data;
using MaintenanceModeMiddleware.Services;

namespace MaintenanceModeMiddleware.Configuration
{
    internal interface IResponseHolder : IOption
    {
        MaintenanceResponse GetResponse(IPathMapperService pathMapperSvc);
    }
}
