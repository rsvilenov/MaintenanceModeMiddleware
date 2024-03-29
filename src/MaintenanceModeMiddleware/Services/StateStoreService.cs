﻿using MaintenanceModeMiddleware.Configuration;
using MaintenanceModeMiddleware.Configuration.State;
using MaintenanceModeMiddleware.StateStore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MaintenanceModeMiddleware.Services
{
    internal class StateStoreService : IStateStoreService
    {
        private readonly List<Type> _optionTypes;

        private readonly IStateStore _stateStore;
        private MaintenanceState _state;

        public StateStoreService(IStateStore stateStore)
        {
            _stateStore = stateStore;
            _optionTypes = CollectOptionTypes();
        }

        MaintenanceState IStateStoreService.GetState()
        {
            if (_state == null)
            {
                if (CanStoreState())
                {
                    RestoreState();
                }
                else
                {
                    _state = new MaintenanceState();
                }
            }

            return _state;
        }

        void IStateStoreService.SetState(MaintenanceState state)
        {
            _state = state;

            if (CanStoreState())
            {
                StoreState();
            }
        }

        private bool CanStoreState()
        {
            return _stateStore != null;
        }

        private void RestoreState()
        {
            StorableMaintenanceState restored = _stateStore.GetState();

            if (restored != null)
            {
                OptionCollection restoredOptions = null;
                if (restored.MiddlewareOptions != null)
                {
                    restoredOptions = new OptionCollection(
                        restored.MiddlewareOptions
                        .Select(o => RestoreOption(o)));
                }

                _state = new MaintenanceState(restored.ExpirationDate,
                    restored.IsMaintenanceOn,
                    restoredOptions);
            }
            else
            {
                _state = new MaintenanceState();
            }
        }

        private void StoreState()
        {
            StorableMaintenanceState storableState = new StorableMaintenanceState
            {
                ExpirationDate = _state.ExpirationDate,
                IsMaintenanceOn = _state.IsMaintenanceOn
            };

            IMiddlewareOptionsContainer optionsContainer = _state;

            storableState.MiddlewareOptions = optionsContainer.MiddlewareOptions
                 ?.GetAll<ISerializableOption>()
                 .Select(o => new StorableOption
                 {
                     StringValue = o.GetStringValue(),
                     TypeName = o.TypeName
                 })
                 .ToList();

            _stateStore.SetState(storableState);
        }

        private ISerializableOption RestoreOption(StorableOption storableOpt)
        {
            Type optionType = _optionTypes.Single(t => t.Name == storableOpt.TypeName);
            ISerializableOption option = (ISerializableOption)Activator.CreateInstance(optionType);
            option.LoadFromString(storableOpt.StringValue);
            return option;
        }

        private List<Type> CollectOptionTypes()
        {
            return
                GetType().Assembly.GetTypes()
                .Where(t => t.GetInterfaces().Contains(typeof(IOption))
                    && !t.IsAbstract
                    && !t.IsInterface)
                .ToList();
        }
    }
}
