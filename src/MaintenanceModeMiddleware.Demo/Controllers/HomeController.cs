using MaintenanceModeMiddleware.TestApp.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;

namespace MaintenanceModeMiddleware.TestApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMaintenanceControlService _maintenanceCtrlSvc;
        public HomeController(IMaintenanceControlService maintenanceCtrlSvc)
        {
            _maintenanceCtrlSvc = maintenanceCtrlSvc;
        }

        public IActionResult Index()
        {
            return View(new HomeViewModel
            {
                IsMaintenanceOn = _maintenanceCtrlSvc.IsMaintenanceModeOn,
                IsEndsOnSpecified = _maintenanceCtrlSvc.EndsOn != null,

                EndsOn = _maintenanceCtrlSvc.EndsOn != null 
                    ? _maintenanceCtrlSvc.EndsOn 
                    : DateTime.Now.AddMinutes(5)
            });
        }

        [HttpPost]
        public IActionResult MaintenanceMode(HomeViewModel vm)
        {
            if (vm.IsMaintenanceOn)
            {
                _maintenanceCtrlSvc.LeaveMaintanence();
            }
            else
            {
                _maintenanceCtrlSvc.EnterMaintanence(vm.IsEndsOnSpecified ? vm.EndsOn : null);
            }

            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
