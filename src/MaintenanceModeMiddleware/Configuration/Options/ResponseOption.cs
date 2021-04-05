namespace MaintenanceModeMiddleware.Configuration.Options
{
    internal class ResponseOption : Option<MaintenanceResponse>
    {
        public ResponseOption(MaintenanceResponse response, bool isDefault = false)
            : base(response, isDefault)
        { }
    }
}
