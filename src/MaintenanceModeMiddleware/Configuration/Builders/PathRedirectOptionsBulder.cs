using MaintenanceModeMiddleware.Configuration.Data;
using System;

namespace MaintenanceModeMiddleware.Configuration.Builders
{
    internal class PathRedirectOptionsBulder : IPathRedirectOptionsBulder
    {
        private readonly PathRedirectData _data;
        private bool _isCustomRetryIntervalSpecified;

        public PathRedirectOptionsBulder()
        {
            _data = new PathRedirectData
            {
                Set503StatusCode = true,
                Code503RetryInterval = DefaultValues.DEFAULT_503_RETRY_INTERVAL
            };
        }

        public void Use503CodeRetryInterval(uint retryInterval)
        {
            _data.Code503RetryInterval = retryInterval;
            _isCustomRetryIntervalSpecified = true;
        }

        public void PreserveStatusCode()
        {
            _data.Set503StatusCode = false;
        }

        public PathRedirectData GetPathRedirectData()
        {
            if (!_data.Set503StatusCode
                && _isCustomRetryIntervalSpecified)
            {
                throw new InvalidOperationException($"{nameof(Use503CodeRetryInterval)} cannot be used along with {nameof(PreserveStatusCode)}.");
            }

            return _data;
        }
    }
}
