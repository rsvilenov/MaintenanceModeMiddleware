using MaintenanceModeMiddleware.Configuration.Data;
using System;

namespace MaintenanceModeMiddleware.Configuration.Builders
{
    public class StatusCodeOptionsBuilder<TBuilder>
        where TBuilder : StatusCodeOptionsBuilder<TBuilder>
    {
        private readonly ResponseStatusCodeData _data;
        private bool _isCustomRetryIntervalSpecified;

        internal StatusCodeOptionsBuilder()
        {
            _data = new ResponseStatusCodeData
            {
                Set503StatusCode = true,
                Code503RetryInterval = DefaultValues.DEFAULT_503_RETRY_INTERVAL
            };
        }

        public TBuilder Use503CodeRetryInterval(uint retryInterval)
        {
            _data.Code503RetryInterval = retryInterval;
            _isCustomRetryIntervalSpecified = true;
            return (TBuilder)this;
        }

        public TBuilder PreserveStatusCode()
        {
            _data.Set503StatusCode = false;
            return (TBuilder)this;
        }

        internal ResponseStatusCodeData GetStatusCodeData()
        {
            if (!_data.Set503StatusCode
                && _isCustomRetryIntervalSpecified)
            {
                throw new InvalidOperationException($"{nameof(Use503CodeRetryInterval)} cannot be used along with {nameof(PreserveStatusCode)}.");
            }

            return _data;
        }
    }

    public class StatusCodeOptionsBuilder : StatusCodeOptionsBuilder<StatusCodeOptionsBuilder>
    {
        internal StatusCodeOptionsBuilder()
        {
        }
    }
}
