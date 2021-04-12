# Middleware configuration

##### Table of Contents  
[General](#general)  
[Options](#options)

## General

You can configure the middleware by passing options during the registration or when calling the method EnterMaintenance() of the injectable IMaintenanceControlService.
The single argument of app.UseMaintenance is a delegate, containing an instance of an option builder class, which you can use to set options. To set an option,
call the method, associated with it. You can see examples of how to do this below.


## Options

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
