using MaintenanceModeMiddleware.StateStore;
using System;

namespace MaintenanceModeMiddleware.Configuration
{
    public class ServiceOptionsBuilder
    {
        public void UseDefaultStateStore()
        {
            _stateStore = GetDefaultStateStore();
            _configured = true;
        }

        public void UseNoStateStore() => _configured = true;

        public void UseStateStore(IStateStore stateStore)
        {
            if (stateStore == null)
            {
                throw new ArgumentNullException(nameof(stateStore));
            }

            _stateStore = stateStore;
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

        private IStateStore _stateStore;
        private bool _configured;
    }
}
