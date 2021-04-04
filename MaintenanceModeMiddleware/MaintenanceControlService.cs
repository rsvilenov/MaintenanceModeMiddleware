using MaintenanceModeMiddleware.Data;
using MaintenanceModeMiddleware.StateStore;
using System;

namespace MaintenanceModeMiddleware
{
    internal class MaintenanceControlService : IMaintenanceControlService, ICanRestoreState
    {
        private readonly IStateStore _stateStore;
        private readonly IServiceProvider _svcProvider;

        private readonly MaintenanceState _state;

        public MaintenanceControlService(IServiceProvider svcProvider, 
            IStateStore stateStore = null)
        {
            _svcProvider = svcProvider;
            _stateStore = stateStore;
            _state = new MaintenanceState();
        }

        public void EnterMaintanence(DateTime? endsOn = null)
        {
            ChangeState(isOn: true, endsOn);
        }

        public void LeaveMaintanence()
        {
            ChangeState(isOn: false);
        }

        public bool IsMaintenanceModeOn
        {
            get
            {
                if (_state.EndsOn <= DateTime.Now)
                {
                    ChangeState(isOn: false);
                }

                return _state.IsMaintenanceOn;
            }
        }

        public DateTime? EndsOn => _state.EndsOn;

        private void ChangeState(bool isOn, DateTime? endsOn = null)
        {
            _state.IsMaintenanceOn = isOn;
            _state.EndsOn = endsOn;

            // store the state
            _stateStore.SetState(_state);
        }

        void ICanRestoreState.RestoreState()
        {
            if (_stateStore == null)
            {
                throw new InvalidOperationException($"No instance of {nameof(IStateStore)} was provided to {nameof(IMaintenanceControlService)}");
            }

            if (_stateStore is IServiceConsumer svcConsumer)
            {
                svcConsumer.ServiceProvider = _svcProvider;
            }

            MaintenanceState restored = _stateStore.GetState();
            if (restored != null)
            {
                _state.EndsOn = restored.EndsOn;
                _state.IsMaintenanceOn = restored.IsMaintenanceOn;
            }
        }
    }
}
