# Configuration

This document describes in detail how to configure this component.

### Table of Contents  
- [General](#general)
    - [Configure in Startup](#Configure-in-Startup)
    - [Configure by the control service](#Configure-by-the-control-service)
    - [Override default configuration values](#Override-default-configuration-values)
- [Options for the control service](#Options-for-the-control-service)
    - [UseNoStateStore](#UseNoStateStore)
    - [UseStateStore](#UseStateStore)
    - [UseDefaultStateStore](#UseDefaultStateStore)
- [Options for the middleware](#Options-for-the-middleware)
    - [BypassAllAuthenticatedUsers](#BypassAllAuthenticatedUsers)
    - [BypassUser](#BypassUser)
    - [BypassUserRole](#BypassUserRole)
    - [BypassUrlPath](#BypassUrlPath)
    - [BypassFileExtension](#BypassFileExtension)
    - [UseResponse](#UseResponse)
    - [UseResponseFile](#UseResponseFile)



## General

The middleware can be configured by passing options during the registration or when calling the method EnterMaintenance() of the injectable IMaintenanceControlService.
The single argument of `app.UseMaintenance` is a delegate, containing an instance of an option builder class, which can be used to set options. To set an option, just
call the method, associated with it. See examples of how to do this below.

### Configure in Startup

To specify where the maintenance state is stored, so that it can be restored after a restart of the application, use the options, available in the extension method for registeratin of the control service. By default, the state is stored in a json file. To override that, you can implement your own state store and pass it as a parameter to UseStateStore(). For inspiration, take a look at the implementation of [FileStateStore](src/MaintenanceModeMiddleware/StateStore/FileStateStore.cs).

```csharp
public void ConfigureServices(IServiceCollection services)
{
...
    services.AddMaintenance(options =>
    {
        options.UseStateStore(myStateStore);
    });
 ```

To configure what parts of the application will be taken down for maintenance and which users will still have access to the entire application, as well as to specify what the exact response in maintenance mode will be, use this:

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
...
// place this before UseEndPoints()
app.UseMaintenance(options =>
{
    options.BypassUser("Demo");
    options.UseResponseFile("maintenance.html", PathBaseDirectory.WebRootPath);
    //... some other options
    // You can configure it using the fluid interface the configuration methods provide. Like this:
    // options.BypassUser("Demo").UseResponseFile("maintenance.html", PathBaseDirectory.WebRootPath);
});

app.UseEndpoints(endpoints =>
...
```

### Configure by the control service

Instead of passing options to `UseMaintenance()`, you can pass them each time when you want to enter maintenance mode. You can even pass different options each time, thus taking down for maintenance different parts of the site one by one. This way, you can implement a user interface, which allows you to specify the options before hitting the button "Enter maintenance". To do that, pass the options to the `EnterMaintenance()` method of the control service:

```csharp
    _maintenanceConrolService.EnterMaintenance(DateTime.Now.AddHours(1), options =>
    {
        options.UseResponse("We will be back in an hour", ContentType.Text, Encoding.UTF8);
    });

```

Notice the first parameter of the call. It allows you to specify the duration of the maintenance. When this datetime mark is reached, the application goes out of maintenance mode automatically.


### Override default configuration values



## Options for the control service

### UseNoStateStore

### UseStateStore

### UseDefaultStateStore


## Options for the middleware

Here is a list of all available options.

### BypassAllAuthenticatedUsers

When this option is set, all authenticated users will have access to the site, while it is in maintenance mode. All others will be served the "maintenance mode" page or response.

```csharp
    options.BypassAllAuthenticatedUsers();
```


### BypassUser

When a user is passed to this option, this user retains full access to the site, while it is in maintenance mode. You can pass multiple users at once by employing the method `BypassUsers(IEnumerable<string>)`.

```csharp
    options.BypassUser("myUserName");
```


### BypassUserRole

Use this to let users of a particular role access the application, while it is in maintenance mode.

```csharp
    options.BypassUserRole("Admin");
```

By default, the option is set for role "Admin".


### BypassUrlPath

The url path (relative to the domain), which starts with the string, specified with this option, remain accessible.

```csharp
    options.BypassUrlPath("/PartNotInMaintenance")
```

or match all paths, beginning with the specified string, in a case-insensitive manner

```csharp
    options.BypassUrlPath("/PartNotInMaintenance", StringComparison.OrdinalIgnoreCase)
```

You can pass multiple paths at once by using `BypassUrlPaths(IEnumerable<string>, StringComparison)`.

### BypassFileExtension

When we are in maintenance mode, we want to continue serving specific file types. For example, the maintenance mode page itself may have images in it, or it may use a separate css and javascript files. By employing this option we can specify which filetypes we want to be exempt from blocking when the application is in maintenance mode.

```csharp
    options.BypassFileExtension(".png");
```

If no extensions are explicitly specified in `BypassFileExtension()` by the user, then these file extensions will remain accessable:
```
"css", "jpg", "png", "gif", "svg", "js"
```
If BypassFileExtension has been used, the extensions above will not be accessible, except if they are specified in the arguments for the option.

You can specify multiple extensions at once by calling this method:
```csharp
    options.BypassFileExtensions(new string[] { "css", "jpg" "mp3" });
```

### UseResponse

Customize the maintenance message to the users.

```csharp
    options.UseResponse("maintenance mode", ContentType.Text, Encoding.UTF8);
```
If the option is not specified, a default html response is served.

### UseResponseFile

Specify a file, containing the maintenance response. This file can be placed either in `ContentRootPath` or in `WebRootPath`.

```csharp
    options.UseResponseFile("customResponse.html", PathBaseDirectory.WebRootPath);
```

If the option is not specified, a default html response is served.
