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
        private readonly IPathMapperService _pathMapperSvc;

        public MaintenanceControlService(
            IPathMapperService pathMapperSvc,
            IStateStoreService stateStoreService,
            Action<IServiceOptionsBuilder> middlewareOptions)
        {
            _pathMapperSvc = pathMapperSvc;
            _stateStoreService = stateStoreService;

            ServiceOptionsBuilder optionsBuilder = new ServiceOptionsBuilder();
            middlewareOptions?.Invoke(optionsBuilder);
            _stateStoreService.SetStateStore(optionsBuilder.GetStateStore());
        }

        public void EnterMaintanence(DateTime? expirationDate = null,
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

        public void LeaveMaintanence()
        {
            _stateStoreService.SetState(new MaintenanceState(isMaintenanceOn: false));
        }

        public IMaintenanceState GetState()
        {
            MaintenanceState state = _stateStoreService.GetState();
            if (state.ExpirationDate <= DateTime.Now)
            {
                LeaveMaintanence();

                state = new MaintenanceState(isMaintenanceOn: false);
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
