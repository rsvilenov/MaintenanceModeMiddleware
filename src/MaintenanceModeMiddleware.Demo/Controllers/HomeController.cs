using MaintenanceModeMiddleware.TestApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MaintenanceModeMiddleware.TestApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMaintenanceControlService _maintenanceCtrlSvc;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public HomeController(IMaintenanceControlService maintenanceCtrlSvc,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            _maintenanceCtrlSvc = maintenanceCtrlSvc;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost]
        public async Task<IActionResult> LoginDemoUser()
        {
            var user = new IdentityUser { UserName = "Demo", Email = "demo@demo.com" };
            var result = await _userManager.CreateAsync(user, "1Password!");
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
            }
            else
            {
                return Error();
            }

            return RedirectToAction(nameof(Index));
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
                _maintenanceCtrlSvc.EnterMaintanence(vm.IsEndsOnSpecified ? vm.EndsOn : null, 
                    options =>
                {
                    options.BypassAllAuthenticatedUsers();
                    options.BypassUrlPath("/");
                });
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
