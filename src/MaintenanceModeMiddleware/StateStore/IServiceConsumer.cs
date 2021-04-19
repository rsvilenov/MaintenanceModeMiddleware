using System;

namespace MaintenanceModeMiddleware.StateStore
{
    /// <summary>
    /// Provides a mechanism for passing dependencies
    /// to the implementations of <see cref="IStateStore">IStateStore</see>
    /// </summary>
    public interface IServiceConsumer
    {
        /// <summary>
        /// This property gets automatically populated with
        /// the application's IServiceProvider. This provides 
        /// the implementations of IStateStore with the ability
        /// to fetch any service, registered in the applicaiton's DI
        /// container, by calling IServiceProvider.GetService(name).
        /// </summary>
        IServiceProvider ServiceProvider { get; internal set; }
    }
}
