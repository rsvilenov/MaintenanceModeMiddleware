using MaintenanceModeMiddleware.TestApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceModeMiddleware.TestApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMaintenanceControlService _maintenanceCtrlSvc;
        public HomeController(ILogger<HomeController> logger, 
            IMaintenanceControlService maintenanceCtrlSvc)
        {
            _logger = logger;
            _maintenanceCtrlSvc = maintenanceCtrlSvc;
        }

        public IActionResult Index()
        {
            return View(new HomeViewModel
            {
                IsMaintenanceOn = _maintenanceCtrlSvc.IsMaintenanceModeOn,
                EndsOn = DateTime.Now.AddMinutes(5)
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
                _maintenanceCtrlSvc.EnterMaintanence(vm.EndsOn);
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
