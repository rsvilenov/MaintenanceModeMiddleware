namespace MaintenanceModeMiddleware.Configuration.Data
{
    internal class FileMaintenanceResponse
    {
        internal FileDescriptor File { get; set; }
        internal int Code503RetryInterval { get; set; }
    }
}
