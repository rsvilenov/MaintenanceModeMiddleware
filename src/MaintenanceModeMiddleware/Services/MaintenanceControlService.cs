using MaintenanceModeMiddleware.Configuration;
using MaintenanceModeMiddleware.Configuration.Builders;
using MaintenanceModeMiddleware.Configuration.State;
using Microsoft.AspNetCore.Hosting;
using System;

namespace MaintenanceModeMiddleware.Services
{
    internal class MaintenanceControlService :
        IMaintenanceControlService
    {
        private readonly IStateStoreService _stateStoreService;
        private readonly IPathMapperService _pathMapperSvc;

        public MaintenanceControlService(
            IPathMapperService pathMapperSvc,
            IStateStoreService stateStoreService,
            Action<ServiceOptionsBuilder> optionBuilderDelegate)
        {
            _pathMapperSvc = pathMapperSvc;
            _stateStoreService = stateStoreService;

            ServiceOptionsBuilder optionsBuilder = new ServiceOptionsBuilder();
            optionBuilderDelegate?.Invoke(optionsBuilder);
            _stateStoreService.SetStateStore(optionsBuilder.GetStateStore());
        }

        public void EnterMaintanence(DateTime? expirationDate = null,
            Action<MiddlewareOptionsBuilder> middlewareOptionsBuilder = null)
        {
            if (_stateStoreService.GetState()
                .IsMaintenanceOn)
            {
                throw new InvalidOperationException("Maintenance mode is already on.");
            }

            OptionCollection newMiddlewareOptions = null;
            if (middlewareOptionsBuilder != null)
            {
                newMiddlewareOptions = GetMiddlewareOptions(middlewareOptionsBuilder);
            }

            _stateStoreService.SetState(new MaintenanceState
            {
                ExpirationDate = expirationDate,
                IsMaintenanceOn = true,
                MiddlewareOptions = newMiddlewareOptions
            });
        }

        public void LeaveMaintanence()
        {
            _stateStoreService.SetState(new MaintenanceState
            {
                IsMaintenanceOn = false
            });
        }

        public MaintenanceState GetState()
        {
            MaintenanceState state = _stateStoreService.GetState();
            if (state.ExpirationDate <= DateTime.Now)
            {
                LeaveMaintanence();

                state = new MaintenanceState
                {
                    IsMaintenanceOn = false
                };
            }

            return state;
        }


        private OptionCollection GetMiddlewareOptions(Action<MiddlewareOptionsBuilder> middlewareOptionsBuilder)
        {
            var optionsBuilder = new MiddlewareOptionsBuilder(_pathMapperSvc);
            middlewareOptionsBuilder?.Invoke(optionsBuilder);

            return optionsBuilder.GetOptions();
        }
    }
}
