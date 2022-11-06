using MaintenanceModeMiddleware.Configuration;

namespace MaintenanceModeMiddleware.Services
{
    internal interface IMaintenanceOptionsService
    {
        OptionCollection GetOptions();
        void SetCurrentOptions(OptionCollection options);
        void SetStartupOptions(OptionCollection options);
    }
}
