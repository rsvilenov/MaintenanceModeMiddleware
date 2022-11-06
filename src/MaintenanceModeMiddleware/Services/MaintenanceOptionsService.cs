using MaintenanceModeMiddleware.Configuration;
using System;

namespace MaintenanceModeMiddleware.Services
{
    internal class MaintenanceOptionsService : IMaintenanceOptionsService
    {
        private OptionCollection _startupOptions;
        private OptionCollection _currentOptions;
        public OptionCollection GetOptions()
        {
            return _currentOptions ?? _startupOptions;
        }

        public void SetCurrentOptions(OptionCollection options)
        {
            _currentOptions = options;
        }

        public void SetStartupOptions(OptionCollection options)
        {
            _startupOptions = options ?? throw new ArgumentNullException(nameof(options));
        }
    }
}
