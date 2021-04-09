using MaintenanceModeMiddleware.Configuration.State;
using MaintenanceModeMiddleware.StateStore;
using System;

namespace MaintenanceModeMiddleware.Tests.HelperTypes
{
    internal class InMemoryStateStore : IStateStore
    {
        private static MaintenanceState _state;
        public MaintenanceState GetState()
        {
            return _state;
        }

        public void SetState(MaintenanceState state)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
        }
    }
}
