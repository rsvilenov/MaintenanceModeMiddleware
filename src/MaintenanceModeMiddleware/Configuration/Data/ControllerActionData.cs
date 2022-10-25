namespace MaintenanceModeMiddleware.Configuration.Data
{
    internal class ControllerActionData
    {
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public string AreaName { get; set; }
        public ResponseStatusCodeData StatusCodeData { get; set; }
    }
}
