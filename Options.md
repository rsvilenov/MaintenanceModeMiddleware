# Middleware options

You can configure the middleware by passing options during the registration or when calling the method EnterMaintenance() of the injectable IMaintenanceControlService.
The single argument of app.UseMaintenance is a delegate, containing an instance of an option builder class, which you can use to set options. To set an option,
call the method, associated with it. You can see examples of how to do this below.


### BypassAllAuthenticatedUsers

When this option is set, all authenticated users will have access to the site, while it is in maintenance mode. All others will be served the "maintenance mode" page or response.

```csharp
app.UseMaintenance(options =>
{
    options.BypassAllAuthenticatedUsers();
}
```


### BypassUser

When a user is passed to this option, this user retains full access to the site, while it is in maintenance mode. You can pass multiple users at once by employing the method BypassUsers(IEnumerable<string>).

```csharp
app.UseMaintenance(options =>
{
    options.BypassUser("myUserName");
}
```


### BypassUserRole

Use this to let users of a particular role access the application, while it is in maintenance mode.

```csharp
app.UseMaintenance(options =>
{
    options.BypassUserRole("Admin");
}
```

By default, the option is set for role "Admin".
