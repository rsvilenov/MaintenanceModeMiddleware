using MaintenanceModeMiddleware.Configuration.Data;
using MaintenanceModeMiddleware.Configuration.Enums;
using MaintenanceModeMiddleware.StateStore;
using System;

namespace MaintenanceModeMiddleware.Configuration.Builders
{
    /// <summary>
    /// A builder for the control service options.
    /// </summary>
    internal class StateStoreOptionsBuilder : IStateStoreOptionsBuilder
    {
        private IStateStore _stateStoreInstance;
        private Type _stateStoreType;
        private bool _useNoStateStore;

        public void UseNoStateStore() => _useNoStateStore = true;

        public void UseStateStore(IStateStore stateStore)
        {
            if (stateStore == null)
            {
                throw new ArgumentNullException(nameof(stateStore));
            }
            _stateStoreInstance = stateStore;
            _stateStoreType = stateStore.GetType();
            _useNoStateStore = true;
        }

        public void UseStateStore<T>()
            where T: IStateStore
        {
            _stateStoreType = typeof(T);
        }

        internal IStateStore GetStateStoreInstance()
        {
            return _stateStoreInstance;
        }

        internal Type GetStateStoreType()
        {
            if (_useNoStateStore)
            {
                return null;
            }

            return _stateStoreType 
                    ?? typeof(FileStateStore);
        }
    }
}
