using System;

namespace MaintenanceModeMiddleware.StateStore
{
    internal interface IServiceConsumer
    {
        IServiceProvider ServiceProvider { get; internal set; }
    }
}
