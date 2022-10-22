namespace MaintenanceModeMiddleware.Configuration.Builders
{
    public interface IPathRedirectOptionsBulder
    {
        /// <summary>
        /// Do not change the response code coming from the redirection path.
        /// If this option is not set, the middleware will change the response code to 503.
        /// </summary>
        void UseDefaultResponseCode();

        /// <summary>
        /// Specify the retry interval for the response code of 503.
        /// </summary>
        /// <param name="retryInterval">The interval in milliseconds</param>
        void Use503CodeRetryInterval(uint retryInterval);
    }
}
