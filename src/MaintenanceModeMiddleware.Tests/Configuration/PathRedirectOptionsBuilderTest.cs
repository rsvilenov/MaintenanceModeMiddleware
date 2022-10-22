﻿using MaintenanceModeMiddleware.Configuration.Builders;
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
            PathRedirectOptionsBulder builder = new PathRedirectOptionsBulder();

            PathRedirectData data = builder.GetPathRedirectData();
                
            data.Set503ResponseCode.ShouldBeTrue();
        }

        [Fact]
        public void WhenCalledWithDefaultOptions_GetPathRedirectData503RetryShouldBeDefault()
        {
            PathRedirectOptionsBulder builder = new PathRedirectOptionsBulder();

            PathRedirectData data = builder.GetPathRedirectData();

            data.Code503RetryInterval
                .ShouldBe(DefaultValues.DEFAULT_503_RETRY_INTERVAL);
        }

        [Fact]
        public void UseDefaultResponseCode_WhenCalled_GetPathRedirectDataSet503ShouldBeFalse()
        {
            PathRedirectOptionsBulder builder = new PathRedirectOptionsBulder();

            builder.UseDefaultResponseCode();

            builder.GetPathRedirectData()
                .Set503ResponseCode
                .ShouldBeFalse();
        }

        [Fact]
        public void Use503CodeRetryInterval_WhenCalledWithNonDefaultInterval_GetPathRedirectDataRetryIntervalShouldMatch()
        {
            const uint retryInterval = 123;
            PathRedirectOptionsBulder builder = new PathRedirectOptionsBulder();
            
            builder.Use503CodeRetryInterval(retryInterval);

            builder.GetPathRedirectData()
                .Code503RetryInterval
                .ShouldBe(retryInterval);
        }

        [Fact]
        public void WhenCalled_UseDefaultResponseCode_And_Use503CodeRetryInterval_ShouldThrow()
        {
            const uint retryInterval = 123;
            PathRedirectOptionsBulder builder = new PathRedirectOptionsBulder();
            builder.UseDefaultResponseCode();
            builder.Use503CodeRetryInterval(retryInterval);

            Action testAction = () => builder.GetPathRedirectData();

            testAction.ShouldThrow<InvalidOperationException>();
        }
    }
}
