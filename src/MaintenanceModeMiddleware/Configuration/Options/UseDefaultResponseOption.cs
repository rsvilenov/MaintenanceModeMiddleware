namespace MaintenanceModeMiddleware.Configuration.Options
{
    internal class UseDefaultResponseOption : Option<bool>
    {
        public UseDefaultResponseOption(bool useDefaultResponse, bool isDefault = false)
            : base(useDefaultResponse, isDefault)
        { }
    }
}
