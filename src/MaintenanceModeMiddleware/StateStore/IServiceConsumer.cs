using System;

namespace MaintenanceModeMiddleware.StateStore
{
    /// <summary>
    /// Provices a mechanism for passing dependencies
    /// to the implementations of <see cref="IStateStore">IStateStore</see>
    /// </summary>
    internal interface IServiceConsumer
    {
        /// <summary>
        /// This property gets automatically populated with
        /// the application's IServiceProvider. This gives the
        /// state store implementations to call IServiceProvider.GetService(name),
        /// getting any service, registered in the application's DI container.
        /// 
        /// Not the prettiest solution, I know...
        /// </summary>
        IServiceProvider ServiceProvider { get; internal set; }
    }
}
