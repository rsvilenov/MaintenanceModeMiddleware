# Configuration

This document describes in detail how to configure this component.

### Table of Contents  
- [General](#general)
    - [Configure in Startup](#Configure-in-Startup)
    - [Configure by the control service](#Configure-by-the-control-service)
    - [Override default configuration values](#Override-default-configuration-values)
- [Options for the middleware](#Options-for-the-middleware)
    - [BypassAllAuthenticatedUsers](#BypassAllAuthenticatedUsers)
    - [BypassUser](#BypassUser)
    - [BypassUserRole](#BypassUserRole)
    - [BypassUrlPath](#BypassUrlPath)
    - [BypassFileExtension](#BypassFileExtension)
    - [UseResponse](#UseResponse)
    - [UseResponseFile](#UseResponseFile)
    - [UseRedirect](#UseRedirect)
    - [UsePathRedirect](#UsePathRedirect)
- [Options for the control service](#Options-for-the-control-service)
    - [UseNoStateStore](#UseNoStateStore)
    - [UseStateStore](#UseStateStore)



## General

The middleware can be configured by passing options during the registration or when calling the method EnterMaintenance() of the injectable IMaintenanceControlService.
The single argument of `app.UseMaintenance` is a delegate, containing an instance of an option builder class, which can be used to set options. To set an option, just
call the method, associated with it. See examples of how to do this below.

### Configure in Startup

To specify where the maintenance state is stored, so that it can be restored after a restart of the application, use the options, available in the extension method for registration of the control service. By default, the state is stored in a json file. To override that, you can implement your own state store and pass it as a parameter to UseStateStore(). For inspiration, take a look at the implementation of [FileStateStore](src/MaintenanceModeMiddleware/StateStore/FileStateStore.cs).

```csharp
public void ConfigureServices(IServiceCollection services)
{
    ...
    services.AddMaintenance(options =>
    {
        options.UseStateStore(myStateStore);
    });
    ...
}
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
        options.UseResponseFile("maintenance.html", EnvDirectory.WebRootPath);
        //... some other options

        // You can configure it using the fluid interface the configuration methods provide. Like this:
        // options.BypassUser("Demo").UseResponseFile("maintenance.html", EnvDirectory.WebRootPath);
    });

    app.UseEndpoints(endpoints =>
    ...
}
```

### Configure by the control service

Instead of passing options to `UseMaintenance()`, you can pass them each time when you want to enter maintenance mode. You can even pass different options each time, thus taking down for maintenance different parts of the site one by one. This way, you can implement a user interface, which allows you to specify the options before hitting the button "Enter maintenance". To do that, pass the options to the `EnterMaintenance()` method of the control service:

```csharp
_maintenanceConrolService.EnterMaintenance(DateTime.Now.AddHours(1), options =>
{
    options.UseResponse("We will be back in an hour", ContentType.Text, Encoding.UTF8);
});

```

Notice the first parameter of the call. It allows you to specify the duration of the maintenance. When this datetime mark is reached, the application goes out of maintenance mode automatically. If you want to enter maintenance for an indefinite time, pass null to this parameter. When you are done with the maintenance, call `LeaveMaintenance()`.


### Override default configuration values

if no options are passed to app.UseMaintenance(), the default configuration would be the same as if the following options were specified:
```csharp
app.UseMaintenance(options =>
{
    options.BypassFileExtensions(new string[] { "css", "jpg", "png", "gif", "svg", "js" });
    options.BypassUrlPath("/Identity");
    options.BypassUserRoles(new string[] { "Admin", "Administrator"});
    options.UseDefaultResponse();
});
```

To override any of these default settings, you have two options:
1. Call the builder's method, which corresponds to the setting, and pass your own value. This will cause the default setting to be omitted in favor of the value you have specified.
2. Call `options.UseNoDefaultValues();` to tell the middleware not to apply any default values. Then you can specify only the settings you need. This is useful for example when you want to allow only a specific user to retain access to the site, regardless of the roles they have.

```csharp
app.UseMaintenance(options =>
{
    options.UseNoDefaultValues(); // this will cause BypassUserRoles(new [] {'Admin'... not to be applied.
    options.BypassUser("John");
    
    // a response should be specified or the
    // middleware will throw an exception.
    options.UseDefaultResponse();
});
```

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

If no extensions are explicitly specified in `BypassFileExtension()` by the user, then these file extensions will remain accessible:
```
"css", "jpg", "png", "gif", "svg", "js"
```
If BypassFileExtension has been used, the extensions above will not be accessible, except if they are specified in the arguments for the option.

You can specify multiple extensions at once by calling this method:
```csharp
    options.BypassFileExtensions(new string[] { "css", "jpg" "mp3" });
```

The static files will still be served, regardless of whether their extensions are specified here, if UseStaticFiles() middleware is placed before the current middleware in the chain.

### UseResponse

Customize the maintenance message to the users.

```csharp
    options.UseResponse("maintenance mode", ContentType.Text, Encoding.UTF8);
```

The default "Retry-After" interval is 5300 milliseconds. If you wish to specify a custom interval, pass the custom value to the last (and optional) parameter of the method. 

```csharp
    options.UseResponse("maintenance mode", ContentType.Text, Encoding.UTF8, 10000);
```

If the option is not specified, a default html response is served.

### UseResponseFile

Specify a file, containing the maintenance response. This file can be placed either in `ContentRootPath` or in `WebRootPath`.

```csharp
    options.UseResponseFile("customResponse.html", EnvDirectory.WebRootPath);
```

The default "Retry-After" interval is 5300 milliseconds. If you wish to specify a custom interval, pass the custom value to the last (and optional) parameter of the method. 

```csharp
    options.UseResponseFile("customResponse.html", EnvDirectory.WebRootPath, 10000);
```

If the option is not specified, a default html response is served.

### UseRedirect

Redirect to a URL when the application is in maintenance mode.

:warning: **Warning:** When this method is used, the response code, returned from the middleware, will not be 503 but 302 instead.

```csharp
    options.UseRedirect("http://my-other-site.com");
```

### UsePathRedirect

Redirect to a URI path when the application is in maintenance mode. The path can lead to anything - an action, a razor page, a static file, etc. 

```csharp
    options.UsePathRedirect("/SomePath");
```

By default the status code of the response, coming from the redirect location, will be overwritten by the middleware - it will be set to 503. If such behaviour is undesired, there is an option to disable it.

```csharp
    options.UsePathRedirect("/SomePath", redirectOptions => 
        redirectOptions.PreserveStatusCode());
```

The default "Retry-After" interval is 5300 milliseconds. You can change it like this:

```csharp
    options.UsePathRedirect("/SomePath", redirectOptions => 
        redirectOptions.Use503CodeRetryInterval(1200));
```

`Use503CodeRetryInterval(uint)` is incompatible with `PreserveStatusCode()`. Using both options at the same time will produce an InvalidOperationException.

## Options for the control service

### UseNoStateStore

To disable the storing of the maintenance state, thus preventing the application to being able to restore it upon restart, use this option.

```csharp
    options.UseNoStateStore();
```
### UseStateStore

To pass a custom implementation of [IStateStore](src/MaintenanceModeMiddleware/StateStore/IStateStore.cs), call this method.

```csharp
    options.UseStateStore<MyCustomStateStore>();
```

To pass an already constructed object of a custom state store, call the method like this:

```csharp
    options.UseStateStore(myCustomStateStore);
```


