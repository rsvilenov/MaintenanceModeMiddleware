using MaintenanceModeMiddleware.Configuration.State;

namespace MaintenanceModeMiddleware.StateStore
{
    /// <summary>
    /// An interface whose implementations provide the logic
    /// for persisting the maintenance state.
    /// </summary>
    public interface IStateStore
    {
        /// <summary>
        /// Encapsulates the logic for restoring the state.
        /// When there is nothing to restore, this method should return null.
        /// </summary>
        /// <returns>An instance of <see cref="StorableMaintenanceState">MaintenanceState</see> or null.</returns>
        StorableMaintenanceState GetState();

        /// <summary>
        /// Encapsulates the logic for storing the state.
        /// </summary>
        /// <param name="state">An instance of <see cref="StorableMaintenanceState">MaintenanceState</see>.</param>
        void SetState(StorableMaintenanceState state);
    }
}
