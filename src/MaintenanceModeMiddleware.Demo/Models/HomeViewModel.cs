using System;

namespace MaintenanceModeMiddleware.TestApp.Models
{
    public class HomeViewModel
    {
        public bool IsMaintenanceOn { get; set; }

        public DateTime? ExpirationDate { get; set; }
        public bool IsExpirationDateSpecified { get; set; }
    }
}
