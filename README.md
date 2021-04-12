# Maintenance Mode Middleware

[![build workflow](https://github.com/rsvilenov/MaintenanceModeMiddleware/actions/workflows/dotnet.yml/badge.svg)](https://github.com/rsvilenov/MaintenanceModeMiddleware/actions/workflows/dotnet.yml)   [![Coverage Status](https://coveralls.io/repos/github/rsvilenov/MaintenanceModeMiddleware/badge.svg?branch=master)](https://coveralls.io/github/rsvilenov/MaintenanceModeMiddleware?branch=master)   [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/coverlet-coverage/coverlet/blob/master/LICENSE)

### Put your ASP.NET Core application (or parts of it) in maintenance mode

We all know that putting a publicly available website or application in maintenance mode is not the perfect thing to do. Reality, however, puts us in situations where this is inevitable. 

Unlike the IIS method, relying on "app_offline.htm", this middleware allows you to keep parts of the site alive while the rest is unaccessable for the public. You can enter maintenance mode from a controller action, for example by pressing a button in the administration area of your applicaiton, or by calling an API. You can specify that the maintenance mode should end automatically after a given period.

Key functionality:
  * Enter and exit maintenance mode by calling a controller action or view method
  * Specify the time for which the maintenance mode should be on
  * Let certain parts of the site remain accessibe while in maintenance mode
  * Let certain users (e.g. admins) still be able to access the entire site (or parts of it)
  * Configure the maintenance mode globally (in Startup.cs) or for each call (in the controller or view action)

### Basic registration

1. Register the middleware in Startup.cs:

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
...
    app.UseMaintenance(options =>
    {
        options.BypassAllAuthenticatedUsers();
        options.BypassUser("Admin");
    });

    app.UseEndpoints(endpoints =>
    {        
...
}
```
Since this is a middleware, the order of registration is important. In order for you to be able to profit from the entire set of features of this middleware, it is recommended that you put its registration just before app.UseEndpoints(..);

2. Register the control service in Startup.cs:

```csharp
public void ConfigureServices(IServiceCollection services)
{
   ...
   services.AddMaintenance();
}
```


### Basic use

Inject the control service in the controller, from which you want to trigger the maintenance mode:

```csharp
private readonly IMaintenanceControlService _maintenanceCtrlSvc;

public HomeController(IMaintenanceControlService maintenanceCtrlSvc)
{
   _maintenanceCtrlSvc = maintenanceCtrlSvc;
}
```

Then just call its methods from the controller actions:

```csharp
[HttpPost]
public IActionResult MaintenanceMode()
{
    if (_maintenanceCtrlSvc.IsMaintenanceOn)
    {
        _maintenanceCtrlSvc.LeaveMaintanence();
    }
    else
    {
        _maintenanceCtrlSvc.EnterMaintanence();
    }

    return RedirectToAction(nameof(Index));
}
```
