namespace MaintenanceModeMiddleware.Configuration.Options
{
    internal class BypassFileExtensionOption : Option<string>
    {
        public BypassFileExtensionOption(string extension, bool isDefault = false)
            : base(extension, isDefault)
        { }
    }
}
