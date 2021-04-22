using MaintenanceModeMiddleware.StateStore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaintenanceModeMiddleware.Configuration.Builders
{
    public interface IServiceOptionsBuilder
    {
        /// <summary>
        /// Store the state in a json file, located in ContentPathRoot.
        /// This is set by default, even if you do not set it.
        /// To override this behavior, use either <see cref="UseNoStateStore"/>
        /// or <see cref="UseStateStore(IStateStore)"/>.
        /// </summary>
        void UseDefaultStateStore();

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
    }
}
