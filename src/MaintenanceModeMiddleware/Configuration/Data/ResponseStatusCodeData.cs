namespace MaintenanceModeMiddleware.Configuration.Data
{
    internal class ResponseStatusCodeData
    {
        public uint Code503RetryInterval { get; set; }
        public bool Set503StatusCode { get; set; }
    }
}
