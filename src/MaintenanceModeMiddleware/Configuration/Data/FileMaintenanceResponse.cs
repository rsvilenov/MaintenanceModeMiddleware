namespace MaintenanceModeMiddleware.Configuration.Data
{
    internal class FileMaintenanceResponse
    {
        internal FileDescriptor File { get; set; }
        internal uint Code503RetryInterval { get; set; }
    }
}
