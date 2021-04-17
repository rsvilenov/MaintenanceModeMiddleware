namespace MaintenanceModeMiddleware.Configuration.Enums
{
    /// <summary>
    /// Contains the signifiers of the available directories
    /// in the web hosting environment.
    /// </summary>
    public enum PathBaseDirectory
    {
        /// <summary>
        /// Corresponds to the absolute path to the directory 
        /// that contains the application content files.
        /// </summary>
        ContentRootPath,

        /// <summary>
        /// Corresponds to the absolute path to the directory
        /// that contains the web-servable application content files.
        /// </summary>
        WebRootPath
    }
}
