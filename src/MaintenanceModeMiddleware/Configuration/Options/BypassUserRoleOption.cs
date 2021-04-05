namespace MaintenanceModeMiddleware.Configuration.Options
{
    internal class BypassUserRoleOption : Option<string>
    {
        public BypassUserRoleOption(string userName, bool isDefault = false)
            : base(userName, isDefault)
        { }
    }
}
