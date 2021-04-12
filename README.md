# Maintenance Mode Middleware

[![build workflow](https://github.com/rsvilenov/MaintenanceModeMiddleware/actions/workflows/dotnet.yml/badge.svg)](https://github.com/rsvilenov/MaintenanceModeMiddleware/actions/workflows/dotnet.yml)   [![Coverage Status](https://coveralls.io/repos/github/rsvilenov/MaintenanceModeMiddleware/badge.svg?branch=master)](https://coveralls.io/github/rsvilenov/MaintenanceModeMiddleware?branch=master)   [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/coverlet-coverage/coverlet/blob/master/LICENSE)

### Put your ASP.NET Core application (or parts of it) in maintenance mode

For the cases when "app_offline.htm" and the staging environment are just not enough.

### Table of Contents  

- [General](#General)
- [Registration](#Registration)
- [Basic use](#Basic-use)
- [Options](#Options)



### General

This component consists of a middleware, which does the actual handling of requests, and a control service, which is used to turn the maintenance mode on and off.

Key functionality:
  * Enter and exit maintenance mode by calling the control service from a controller action or view method
  * Specify the time for which the maintenance mode should be on
  * Let certain parts of the site remain accessibe while in maintenance mode
  * Let certain users (e.g. admins) still be able to access the entire site
  * Configure the maintenance mode globally (in Startup.cs) or for each call (in the controller or view action)
  * Customize your maintenance mode response (html, text and json files/data are supported)
  * SEO friendly, as it relies on response code 503 with a "Retry-After" interval

### Registration

* Register the middleware in Startup.cs:

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
...
    app.UseMaintenance(options =>
    {
        options.BypassUserRole("Admin");
    });

    app.UseEndpoints(endpoints =>
    {        
...
}
```
Since this is a middleware, the order of registration is important. In order for you to be able to profit from the entire set of features of this middleware, it is highly recommended that you put its registration just before app.UseEndpoints(..);

* Register the control service in Startup.cs:

```csharp
public void ConfigureServices(IServiceCollection services)
{
   ...
   services.AddMaintenance();
}
```

You can pass options to the control service as well. For example, you can specify that the maintenance state should be preserved after the app has been restarted. By default the state is stored in a json file. If you want to store the state somewhere else, for example in the database, you can pass your own implementation of the IStateStore interface to optoins.UseStateStore(yourStateStore).

### Basic use

Inject the control service in the controller, from which you want to trigger the maintenance mode:

```csharp
private readonly IMaintenanceControlService _maintenanceCtrlSvc;

public AdminController(IMaintenanceControlService maintenanceCtrlSvc)
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

⚠️ **Note**: The identity area remains accessible in maintenance mode so that you can log in with the users, for which the site should be visible. You can block the access to this area as well. Read how to do that in the full [documentation](Configuration.md).


### Options

All the available options are decsribed in detail here [here](Configuration.md).

### Thanks to

This project was inspired from this blog [post](https://rimdev.io/middleware-madness-site-maintenance-in-aspnet-core/), written by Khalid Abuhakmeh abd Bill Boga.
