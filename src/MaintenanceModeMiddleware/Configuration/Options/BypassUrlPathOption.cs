namespace MaintenanceModeMiddleware.Configuration.Options
{
    internal class BypassUrlPathOption : Option<UrlPath>
    {
        public BypassUrlPathOption(UrlPath path, bool isDefault = false)
            : base(path, isDefault)
        { }
    }
}
