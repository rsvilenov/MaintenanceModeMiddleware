# Maintenance Mode Middleware

[![build workflow](https://github.com/rsvilenov/MaintenanceModeMiddleware/actions/workflows/dotnet.yml/badge.svg)](https://github.com/rsvilenov/MaintenanceModeMiddleware/actions/workflows/dotnet.yml)   [![Coverage Status](https://coveralls.io/repos/github/rsvilenov/MaintenanceModeMiddleware/badge.svg?branch=master)](https://coveralls.io/github/rsvilenov/MaintenanceModeMiddleware?branch=master)   [![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)   [![nuget](https://img.shields.io/nuget/v/MaintenanceModeMiddleware/1.0.2)](https://www.nuget.org/packages/MaintenanceModeMiddleware/1.0.2)

### Enables maintenance mode in your ASP.NET Core application

For the cases when "app_offline.htm" and the staging environment are just not enough.

> Supports ASP.NET Core 3.1 and 5.0 

### Table of Contents  

- [General](#General)
- [Installation](#Installation)
- [Registration](#Registration)
- [Basic use](#Basic-use)
- [Options](#Options)



### General

This component consists of a middleware, which does the actual handling of requests, and a control service, which is used to turn the maintenance mode on and off.

Key features:
  * Enter and exit maintenance mode by using the injectable control service.
  * Specify an expiration date on which maintenance mode should turn off automatically.
  * Let certain parts of the site remain accessibe while maintenance mode is on.
  * Let certain users (e.g. admins) still be able to access the entire site.
  * Configure the component globally (in Startup.cs), or...
  * Specify a different configuration every time the application enters maintenance mode.
  * Customize your maintenance mode response (html, text and json files/data are supported).
  * Don't worry about SEO problems - the component sends response code 503 with a "Retry-After" interval.

### Installation

You can view the [package page on NuGet](hhttps://www.nuget.org/packages/MaintenanceModeMiddleware/).

To install `MaintenanceModeMiddleware`, run the following command in the Package Manager Console:

```
PM> Install-Package MaintenanceModeMiddleware
```

### Registration

* Register the middleware in Startup.cs:

```csharp
using MaintenanceModeMiddleware.Extensions;

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
:information_source: Since this is a middleware, the order of registration is important. In order for you to be able to profit from the entire set of features of this middleware, it is highly recommended that you put its registration just before `app.UseEndpoints(..);`.

* Register the control service in Startup.cs:

```csharp
public void ConfigureServices(IServiceCollection services)
{
   ...
   services.AddMaintenance();
}
```

You can pass options to the control service as well. For example, you can specify that the maintenance state should be preserved after the app has been restarted. By default the state is stored in a json file. If you want to store the state somewhere else, for example in the database, you can pass your own implementation of the `IStateStore` interface to `optoins.UseStateStore(yourStateStore)`.

### Basic use

Inject the control service in the controller, from which you want to trigger the maintenance mode:

```csharp
using MaintenanceModeMiddleware.Services;

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
    var maintenanceState = _maintenanceCtrlSvc.GetState();

    if (maintenanceState.IsMaintenanceOn)
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

:information_source: **Note**: The identity area remains accessible in maintenance mode so that you can log in with the users, for which the site should be visible. You can block the access to this area as well. Read how to do that in the full [documentation](Configuration.md).


### Options

All the available options are decsribed in detail here [here](Configuration.md).

### Thanks to

This project was inspired from this blog [post](https://rimdev.io/middleware-madness-site-maintenance-in-aspnet-core/), written by Khalid Abuhakmeh abd Bill Boga.
