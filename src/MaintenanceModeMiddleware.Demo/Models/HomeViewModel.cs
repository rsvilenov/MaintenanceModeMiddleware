using System;

namespace MaintenanceModeMiddleware.TestApp.Models
{
    public class HomeViewModel
    {
        public bool IsMaintenanceOn { get; set; }

        public DateTime? EndsOn { get; set; }
        public bool IsEndsOnSpecified { get; set; }
    }
}
