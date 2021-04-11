using MaintenanceModeMiddleware.Configuration;
using MaintenanceModeMiddleware.Configuration.Builders;
using MaintenanceModeMiddleware.Configuration.Options;
using MaintenanceModeMiddleware.Configuration.State;
using MaintenanceModeMiddleware.StateStore;
using System;
using System.Linq;

namespace MaintenanceModeMiddleware
{
    internal class MaintenanceControlService : 
        IMaintenanceControlService, ICanRestoreState, ICanOverrideMiddlewareOptions
    {
        private readonly IStateStore _stateStore;
        private readonly IServiceProvider _svcProvider;

        private readonly MaintenanceState _state;
        private OptionCollection _middlewareOptionsToOverride;

        public MaintenanceControlService(IServiceProvider svcProvider, 
            Action<ServiceOptionsBuilder> optionBuilderDelegate)
        {
            _svcProvider = svcProvider;

            ServiceOptionsBuilder optionsBuilder = new ServiceOptionsBuilder();
            optionBuilderDelegate?.Invoke(optionsBuilder);
            _stateStore = optionsBuilder.GetStateStore();

            _state = new MaintenanceState();
        }

        public void EnterMaintanence(DateTime? endsOn = null,
            Action<MiddlewareOptionsBuilder> middlewareOptions = null)
        {
            if (_state.IsMaintenanceOn)
            {
                throw new InvalidOperationException("Maintenance mode is already on.");
            }

            OptionCollection optionsToOverride = null;
            if (middlewareOptions != null)
            {
                MiddlewareOptionsBuilder optionsBuilder = new MiddlewareOptionsBuilder();
                middlewareOptions?.Invoke(optionsBuilder);

                optionsToOverride = optionsBuilder.GetOptions();
            }

            ChangeState(isOn: true, endsOn, optionsToOverride);
        }

        public void LeaveMaintanence()
        {
            _middlewareOptionsToOverride = null;
            ChangeState(isOn: false, endsOn: null, middlewareOptions: null);
        }

        public bool IsMaintenanceModeOn
        {
            get
            {
                if (_state.EndsOn <= DateTime.Now)
                {
                    ChangeState(isOn: false, endsOn: null, middlewareOptions: null);
                }

                return _state.IsMaintenanceOn;
            }
        }

        public DateTime? EndsOn => _state.EndsOn;

        private void ChangeState(bool isOn, DateTime? endsOn = null, OptionCollection middlewareOptions = null)
        {
            _middlewareOptionsToOverride = middlewareOptions;

            _state.MiddlewareOptions = middlewareOptions
                ?.GetAll<IOption>()
                .Select(o => new StorableOption
                {
                    StringValue = o.GetStringValue(),
                    TypeName = o.TypeName
                })
                .ToList();

            _state.IsMaintenanceOn = isOn;
            _state.EndsOn = endsOn;

            // store the state
            _stateStore?.SetState(_state);
        }

        void ICanRestoreState.RestoreState()
        {
            if (_stateStore != null)
            {
                if (_stateStore is IServiceConsumer svcConsumer)
                {
                    svcConsumer.ServiceProvider = _svcProvider;
                }

                MaintenanceState restored = _stateStore.GetState();
                if (restored != null)
                {
                    _state.EndsOn = restored.EndsOn;
                    _state.IsMaintenanceOn = restored.IsMaintenanceOn;
                    if (restored.MiddlewareOptions != null)
                    {
                        _state.MiddlewareOptions = restored.MiddlewareOptions;

                        _middlewareOptionsToOverride = new OptionCollection(
                            _state.MiddlewareOptions
                            .Select(o => RestoreOption(o)));
                    }
                }
            }
        }

        private IOption RestoreOption(StorableOption storableOpt)
        {
            Type optionType = Type.GetType($"{GetType().Namespace}.Configuration.Options.{storableOpt.TypeName}");
            IOption option = (IOption)Activator.CreateInstance(optionType);
            option.LoadFromString(storableOpt.StringValue);
            return option;
        }

        OptionCollection ICanOverrideMiddlewareOptions.GetOptionsToOverride()
        {
            // return a copy
            return _middlewareOptionsToOverride != null 
                ? new OptionCollection(_middlewareOptionsToOverride.GetAll()) 
                : null;
        }
    }
}
