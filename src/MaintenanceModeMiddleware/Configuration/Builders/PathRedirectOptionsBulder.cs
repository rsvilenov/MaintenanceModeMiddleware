using MaintenanceModeMiddleware.Configuration.Data;

namespace MaintenanceModeMiddleware.Configuration.Builders
{
    internal class PathRedirectOptionsBulder : IPathRedirectOptionsBulder
    {
        private readonly PathRedirectData _data;

        public PathRedirectOptionsBulder()
        {
            _data = new PathRedirectData();
            _data.Set503ResponseCode = true;
        }

        public void Use503CodeRetryInterval(int retryInterval)
        {
            _data.Code503RetryInterval = retryInterval;
        }

        public void UseDefaultResponseCode()
        {
            _data.Set503ResponseCode = false;
        }

        public PathRedirectData GetPathRedirectData()
        {
            return _data;
        }
    }
}
