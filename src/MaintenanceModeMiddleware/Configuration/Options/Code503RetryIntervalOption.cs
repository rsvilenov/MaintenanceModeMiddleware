namespace MaintenanceModeMiddleware.Configuration.Options
{
    internal class Code503RetryIntervalOption : Option<int>
    {
        public Code503RetryIntervalOption(int interval, bool isDefault = false)
            : base(interval, isDefault)
        { }
    }
}
