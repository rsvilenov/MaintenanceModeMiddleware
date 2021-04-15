using MaintenanceModeMiddleware.Configuration.State;
using MaintenanceModeMiddleware.StateStore;
using System;

namespace MaintenanceModeMiddleware.Tests.HelperTypes
{
    internal class InMemoryStateStore : IStateStore
    {
        private static StorableMaintenanceState _state;
        public StorableMaintenanceState GetState()
        {
            return _state;
        }

        public void SetState(StorableMaintenanceState state)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
        }
    }
}
