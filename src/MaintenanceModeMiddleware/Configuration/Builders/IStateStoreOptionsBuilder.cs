using MaintenanceModeMiddleware.StateStore;

namespace MaintenanceModeMiddleware.Configuration.Builders
{
    public interface IStateStoreOptionsBuilder
    {
        /// <summary>
        /// Do not preserve the maintenance state upon a restart of the application.
        /// </summary>
        void UseNoStateStore();

        /// <summary>
        /// Allows passing a custom state store, which can,
        /// for example, store the maintenance state in the database.
        /// </summary>
        /// <param name="stateStore">The custom implementation of <see cref="IStateStore"/></param>
        void UseStateStore(IStateStore stateStore);

        /// <summary>
        /// Allows passing a custom state store, which can,
        /// for example, store the maintenance state in the database.
        /// </summary>
        /// <typeparam name="T">The type of the custom implementation of <see cref="IStateStore"/></typeparam>
        void UseStateStore<T>() where T : IStateStore;
    }
}
