using MaintenanceModeMiddleware.Configuration;
using MaintenanceModeMiddleware.Configuration.Builders;
using MaintenanceModeMiddleware.Configuration.State;
using System;

namespace MaintenanceModeMiddleware.Services
{
    internal class MaintenanceControlService :
        IMaintenanceControlService
    {
        private readonly IStateStoreService _stateStoreService;
        private readonly IDirectoryMapperService _dirMapperSvc;

        public MaintenanceControlService(
            IDirectoryMapperService dirMapperSvc,
            IStateStoreService stateStoreService)
        {
            _dirMapperSvc = dirMapperSvc;
            _stateStoreService = stateStoreService;
        }

        public void EnterMaintenance(DateTime? expirationDate = null,
            Action<IMiddlewareOptionsBuilder> middlewareOptionsBuilder = null)
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

            _stateStoreService.SetState(new MaintenanceState(expirationDate,
                isMaintenanceOn: true,
                newMiddlewareOptions));
        }

        public void LeaveMaintenance()
        {
            _stateStoreService.SetState(new MaintenanceState(isMaintenanceOn: false));
        }

        public IMaintenanceState GetState()
        {
            MaintenanceState state = _stateStoreService.GetState();
            if (state.ExpirationDate <= DateTime.Now)
            {
                LeaveMaintenance();

                state = new MaintenanceState(isMaintenanceOn: false);
            }

            return state;
        }


        private OptionCollection GetMiddlewareOptions(Action<MiddlewareOptionsBuilder> middlewareOptionsBuilder)
        {
            var optionsBuilder = new MiddlewareOptionsBuilder(_dirMapperSvc);
            middlewareOptionsBuilder?.Invoke(optionsBuilder);

            return optionsBuilder.GetOptions();
        }
    }
}
