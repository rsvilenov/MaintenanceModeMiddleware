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

        public void UseStateStore<T>(T instance = null)
            where T: class, IStateStore
        {
            if (typeof(T) == typeof(IStateStore)
                && instance == null)
            {
                throw new ArgumentNullException(nameof(IStateStore));
            }

            _stateStoreType = typeof(T);
            _stateStoreInstance = instance;
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
