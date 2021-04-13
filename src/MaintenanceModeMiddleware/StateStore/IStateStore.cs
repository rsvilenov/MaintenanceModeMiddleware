using MaintenanceModeMiddleware.Configuration.State;

namespace MaintenanceModeMiddleware.StateStore
{
    /// <summary>
    /// An interface for 
    /// </summary>
    public interface IStateStore
    {
        /// <summary>
        /// Encapsulates the logic for restoring the state.
        /// When there is nothing to restore, this method should return null.
        /// </summary>
        /// <returns>An instance of <see cref="MaintenanceState">MaintenanceState</see> or null.</returns>
        MaintenanceState GetState();

        /// <summary>
        /// Encapsulates the logic for storing the state.
        /// </summary>
        /// <param name="state">An instance of <see cref="MaintenanceState">MaintenanceState</see>.</param>
        void SetState(MaintenanceState state);
    }
}
