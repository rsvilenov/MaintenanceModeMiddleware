using System;
using System.ComponentModel.DataAnnotations;

namespace MaintenanceModeMiddleware.TestApp.Models
{
    public class HomeViewModel
    {
        public bool IsMaintenanceOn { get; set; }

        public DateTime? EndsOn { get; set; }
    }
}
