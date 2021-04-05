namespace MaintenanceModeMiddleware.Configuration.Options
{
    internal class ResponseFileOption : Option<FileDescriptor>
    {
        public ResponseFileOption(FileDescriptor file, bool isDefault = false)
            : base(file, isDefault)
        { }
    }
}
