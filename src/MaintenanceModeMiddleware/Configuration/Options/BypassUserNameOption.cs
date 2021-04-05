namespace MaintenanceModeMiddleware.Configuration.Options
{
    internal class BypassUserNameOption : Option<string>
    {
        public BypassUserNameOption(string userName, bool isDefault = false)
            : base(userName, isDefault)
        {}
    }
}
