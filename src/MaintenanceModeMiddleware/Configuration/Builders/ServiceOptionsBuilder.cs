using MaintenanceModeMiddleware.Configuration.Data;
using MaintenanceModeMiddleware.Configuration.Enums;
using MaintenanceModeMiddleware.StateStore;
using System;

namespace MaintenanceModeMiddleware.Configuration.Builders
{
    /// <summary>
    /// A builder for the control service options.
    /// </summary>
    public class ServiceOptionsBuilder
    {
        private IStateStore _stateStore;
        private bool _stateStoreSelected;

        
        /// <summary>
        /// Store the state in a json file, located in ContentPathRoot.
        /// This is set by default, even if you do not set it.
        /// To override this behavior, use either <see cref="UseNoStateStore"/>
        /// or <see cref="UseStateStore(IStateStore)"/>.
        /// </summary>
        public void UseDefaultStateStore()
        {
            _stateStore = GetDefaultStateStore();
            _stateStoreSelected = true;
        }

        /// <summary>
        /// Do not preserve the maintenance state upon a restart of the application.
        /// </summary>
        public void UseNoStateStore() => _stateStoreSelected = true;

        /// <summary>
        /// Allows passing a custom state store, which can,
        /// for example, store the maintenance state in the database.
        /// </summary>
        /// <param name="stateStore"></param>
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
                EnvDirectory.ContentRootPath));
        }
    }
}
