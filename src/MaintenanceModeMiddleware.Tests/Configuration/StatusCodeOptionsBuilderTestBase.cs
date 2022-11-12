using MaintenanceModeMiddleware.Configuration.Builders;
using MaintenanceModeMiddleware.Configuration.Data;
using Shouldly;
using System;
using Xunit;

namespace MaintenanceModeMiddleware.Tests.Configuration
{
    public abstract class StatusCodeOptionsBuilderTestBase : IClassFixture<IStatusCodeOptionsBuilder>
    {
        private readonly IStatusCodeOptionsBuilder _builder;
        public StatusCodeOptionsBuilderTestBase(IStatusCodeOptionsBuilder builder)
        {
            _builder = builder;
        }

        [Fact]
        public void WhenCalledWithDefaultOptions_GetPathRedirectDataSet503ShouldBeTrue()
        {
            ResponseStatusCodeData data = GetResponseStatusCodeData();
                
            data.Set503StatusCode.ShouldBeTrue();
        }

        [Fact]
        public void WhenCalledWithDefaultOptions_GetPathRedirectData503RetryShouldBeDefault()
        {
            ResponseStatusCodeData data = GetResponseStatusCodeData();

            data.Code503RetryInterval
                .ShouldBe(DefaultValues.DEFAULT_503_RETRY_INTERVAL);
        }

        [Fact]
        public void PreserveStatusCode_WhenCalled_GetPathRedirectDataSet503ShouldBeFalse()
        {
            _builder.PreserveStatusCode();

            GetResponseStatusCodeData()
                .Set503StatusCode
                .ShouldBeFalse();
        }

        [Fact]
        public void Use503CodeRetryInterval_WhenCalledWithNonDefaultInterval_GetPathRedirectDataRetryIntervalShouldMatch()
        {
            const uint retryInterval = 123;

            _builder.Use503CodeRetryInterval(retryInterval);

            GetResponseStatusCodeData()
                .Code503RetryInterval
                .ShouldBe(retryInterval);
        }

        [Fact]
        public void WhenCalled_PreserveStatusCode_And_Use503CodeRetryInterval_ShouldThrow()
        {
            const uint retryInterval = 123;
            _builder.PreserveStatusCode();
            _builder.Use503CodeRetryInterval(retryInterval);

            Action testAction = () => GetResponseStatusCodeData();

            testAction.ShouldThrow<InvalidOperationException>();
        }

        private ResponseStatusCodeData GetResponseStatusCodeData()
        {
            return ((StatusCodeOptionsBuilder)_builder).GetStatusCodeData();
        }
    }
}
