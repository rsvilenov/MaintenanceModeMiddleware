namespace MaintenanceModeMiddleware.Configuration
{
    internal interface IHaveStatusCodeInfo : IOption
    {
        uint Code503RetryInterval { get; }
        bool Set503StatusCode { get; }
    }
}
