namespace MaintenanceModeMiddleware.Configuration
{
    internal interface IRedirectInitializer : IOption
    {
        string RedirectLocation { get; }
    }
}
