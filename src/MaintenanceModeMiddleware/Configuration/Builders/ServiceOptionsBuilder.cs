using MaintenanceModeMiddleware.Configuration.Data;
using MaintenanceModeMiddleware.Configuration.Enums;
using MaintenanceModeMiddleware.StateStore;
using System;

namespace MaintenanceModeMiddleware.Configuration.Builders
{
    public class ServiceOptionsBuilder
    {
        private IStateStore _stateStore;
        private bool _configured;

        public void UseDefaultStateStore()
        {
            _stateStore = GetDefaultStateStore();
            _configured = true;
        }

        public void UseNoStateStore() => _configured = true;

        public void UseStateStore(IStateStore stateStore)
        {
            _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
            _configured = true;
        }

        internal IStateStore GetStateStore()
        {
            return _stateStore ?? (_configured ? null : GetDefaultStateStore());
        }

        private IStateStore GetDefaultStateStore()
        {
            return new FileStateStore(new FileDescriptor("maintenanceState.json",
                PathBaseDirectory.ContentRootPath));
        }
    }
}
