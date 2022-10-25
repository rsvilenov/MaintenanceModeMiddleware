using MaintenanceModeMiddleware.Configuration.Builders;
using MaintenanceModeMiddleware.Configuration.Data;
using Shouldly;
using System;
using Xunit;

namespace MaintenanceModeMiddleware.Tests.Configuration
{
    public class PathRedirectOptionsBuilderTest
    {
        [Fact]
        public void WhenCalledWithDefaultOptions_GetPathRedirectDataSet503ShouldBeTrue()
        {
            StatusCodeOptionsBulder builder = new StatusCodeOptionsBulder();

            ResponseStatusCodeData data = builder.GetStatusCodeData();
                
            data.Set503StatusCode.ShouldBeTrue();
        }

        [Fact]
        public void WhenCalledWithDefaultOptions_GetPathRedirectData503RetryShouldBeDefault()
        {
            StatusCodeOptionsBulder builder = new StatusCodeOptionsBulder();

            ResponseStatusCodeData data = builder.GetStatusCodeData();

            data.Code503RetryInterval
                .ShouldBe(DefaultValues.DEFAULT_503_RETRY_INTERVAL);
        }

        [Fact]
        public void PreserveStatusCode_WhenCalled_GetPathRedirectDataSet503ShouldBeFalse()
        {
            StatusCodeOptionsBulder builder = new StatusCodeOptionsBulder();

            builder.PreserveStatusCode();

            builder.GetStatusCodeData()
                .Set503StatusCode
                .ShouldBeFalse();
        }

        [Fact]
        public void Use503CodeRetryInterval_WhenCalledWithNonDefaultInterval_GetPathRedirectDataRetryIntervalShouldMatch()
        {
            const uint retryInterval = 123;
            StatusCodeOptionsBulder builder = new StatusCodeOptionsBulder();
            
            builder.Use503CodeRetryInterval(retryInterval);

            builder.GetStatusCodeData()
                .Code503RetryInterval
                .ShouldBe(retryInterval);
        }

        [Fact]
        public void WhenCalled_PreserveStatusCode_And_Use503CodeRetryInterval_ShouldThrow()
        {
            const uint retryInterval = 123;
            StatusCodeOptionsBulder builder = new StatusCodeOptionsBulder();
            builder.PreserveStatusCode();
            builder.Use503CodeRetryInterval(retryInterval);

            Action testAction = () => builder.GetStatusCodeData();

            testAction.ShouldThrow<InvalidOperationException>();
        }
    }
}
