namespace MaintenanceModeMiddleware.Configuration.Options
{
    internal class BypassAuthenticatedUsersOption : Option<bool>
    {
        public BypassAuthenticatedUsersOption(bool bypass, bool isDefault = false)
            : base(bypass, isDefault)
        { }
    }
}
