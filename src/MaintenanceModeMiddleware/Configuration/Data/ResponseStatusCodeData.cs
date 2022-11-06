namespace MaintenanceModeMiddleware.Configuration.Data
{
    internal class ResponseStatusCodeData : IHaveStatusCodeInfo
    {
        public uint Code503RetryInterval { get; set; }
        public bool Set503StatusCode { get; set; }
    }
}
