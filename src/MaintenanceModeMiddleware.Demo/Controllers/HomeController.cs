﻿using MaintenanceModeMiddleware.Services;
using MaintenanceModeMiddleware.TestApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Linq;
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

        

        public IActionResult Index()
        {
            var maintenanceState = _maintenanceCtrlSvc.GetState();

            var viewModel = new HomeViewModel
            {
                IsMaintenanceOn = maintenanceState.IsMaintenanceOn,
                IsExpirationDateSpecified = maintenanceState.ExpirationDate != null,

                ExpirationDate = maintenanceState.ExpirationDate != null
                    ? maintenanceState.ExpirationDate
                    : DateTime.Now.AddMinutes(5)
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult ToggleMaintenanceMode(HomeViewModel vm)
        {
            if (vm.IsMaintenanceOn)
            {
                _maintenanceCtrlSvc.LeaveMaintenance();
            }
            else
            {
                _maintenanceCtrlSvc.EnterMaintenance(vm.IsExpirationDateSpecified ? vm.ExpirationDate : null,
                    options =>
                    {
                        options.BypassAllAuthenticatedUsers();
                        // uncomment this to use custom maintenance response path/action
                        //options.UsePathRedirect("/Home/Maintenance");
                    });
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Authenticate()
        {
            IdentityUser user = await _userManager.FindByNameAsync("Demo");
            if (user == null)
            {
                user = new IdentityUser { UserName = "Demo", Email = "demo@demo.com" };
                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    string errors = result.Errors
                        .Select(e => e.Description)
                        .Aggregate((s1, s2) => s1 + Environment.NewLine + s2);

                    throw new InvalidOperationException($"Could not create a demo user: {errors}");
                }
            }

            await _signInManager.SignInAsync(user, isPersistent: false);

            return RedirectToAction(nameof(Index));
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// This is an example action to use with
        /// options.UsePathRedirect("/Home/Maintenance")
        /// </summary>
        /// <returns></returns>
        public IActionResult Maintenance()
        {
            return View();
        }
    }
}
