using MaintenanceModeMiddleware.Configuration.Data;
using MaintenanceModeMiddleware.Configuration.Enums;
using MaintenanceModeMiddleware.StateStore;
using System;

namespace MaintenanceModeMiddleware.Configuration.Builders
{
    public class ServiceOptionsBuilder
    {
        private IStateStore _stateStore;
        private bool _stateStoreSelected;

        public void UseDefaultStateStore()
        {
            _stateStore = GetDefaultStateStore();
            _stateStoreSelected = true;
        }

        public void UseNoStateStore() => _stateStoreSelected = true;

        public void UseStateStore(IStateStore stateStore)
        {
            _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
            _stateStoreSelected = true;
        }

        internal IStateStore GetStateStore()
        {
            return _stateStore ?? (_stateStoreSelected ? null : GetDefaultStateStore());
        }

        private IStateStore GetDefaultStateStore()
        {
            return new FileStateStore(new FileDescriptor("maintenanceState.json",
                PathBaseDirectory.ContentRootPath));
        }
    }
}
