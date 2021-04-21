using System;

namespace MaintenanceModeMiddleware.Configuration.State
{
    internal class MaintenanceState : IMaintenanceState, IMiddlewareOptionsContainer
    {
        private readonly OptionCollection _middlewareOptions;

        internal MaintenanceState() { }

        internal MaintenanceState(bool isMaintenanceOn)
        : this(null, isMaintenanceOn, null) 
        { }

        internal MaintenanceState(DateTime? expirationDate, 
            bool isMaintenanceOn, 
            OptionCollection middlewareOptions) 
        {
            ExpirationDate = expirationDate;
            IsMaintenanceOn = isMaintenanceOn;
            _middlewareOptions = middlewareOptions;
        }

        public DateTime? ExpirationDate { get; internal set; }

        public bool IsMaintenanceOn { get; internal set; }

        OptionCollection IMiddlewareOptionsContainer.MiddlewareOptions => _middlewareOptions;
    }
}
