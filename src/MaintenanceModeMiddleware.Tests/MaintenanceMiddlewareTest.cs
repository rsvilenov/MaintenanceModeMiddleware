using MaintenanceModeMiddleware.Configuration;
using MaintenanceModeMiddleware.Configuration.Builders;
using MaintenanceModeMiddleware.Configuration.Enums;
using MaintenanceModeMiddleware.Configuration.Options;
using MaintenanceModeMiddleware.Tests.HelperTypes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using Shouldly;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions.Ordering;

namespace MaintenanceModeMiddleware.Tests
{
    public class MaintenanceMiddlewareTest
    {

        [Fact]
        public void Constructor_DefaultOptions()
        {
            bool isBuilderCalled = false;
            Action<MiddlewareOptionsBuilder> optionBuilder = (options) =>
            {
                options.FillEmptyOptionsWithDefault();
                isBuilderCalled = true;
            };

            Action testAction = () =>
                new MaintenanceMiddleware(null,
                    null,
                    null,
                    optionBuilder);

            testAction.ShouldNotThrow();
            isBuilderCalled.ShouldBeTrue();
        }

        [Theory]
        [InlineData(true, true, null, null)]
        [InlineData(false, true, typeof(ArgumentException), "No response was specified")]
        [InlineData(true, false, typeof(ArgumentException), "No value was specified for 503")]
        public void Constructor_MandatoryOptions(
            bool responseSpecified, 
            bool retryIntervalSpecified,
            Type expectedException, 
            string expectedExMsgStart)
        {
            Action<MiddlewareOptionsBuilder> optionBuilder = (options) =>
            {
                if (responseSpecified)
                {
                    options.UseDefaultResponse();
                }

                if (retryIntervalSpecified)
                {
                    options.Set503RetryAfterInterval(1000);
                }
            };
            
            Action testAction = () => 
                new MaintenanceMiddleware(null,
                    null,
                    null,
                    optionBuilder);

            if (expectedException == null)
            {
                testAction.ShouldNotThrow();
            }
            else
            {
                testAction.ShouldThrow(expectedException)
                    .Message.ShouldStartWith(expectedExMsgStart);
            }
        }

        const string testFileName = "test.txt";
        [Theory]
        [InlineData(testFileName, null, null)]
        [InlineData("nonexistent.txt", typeof(ArgumentException), "Could not find file")]
        public void Constructor_ResponseFileOption(
            string filePath,
            Type expectedException,
            string expectedExMsgStart)
        {
            IWebHostEnvironment webHostEnv = Substitute.For<IWebHostEnvironment>();
            webHostEnv.ContentRootPath.Returns(Path.GetTempPath());
            File.Create(Path.Combine(webHostEnv.ContentRootPath, testFileName))
                .Dispose();

            Action<MiddlewareOptionsBuilder> optionBuilder = (options) =>
            {
                options.UseResponseFile(filePath, PathBaseDirectory.ContentRootPath);
                // prevent other exceptions due to missing required options
                options.FillEmptyOptionsWithDefault();
            };

            Action testAction = () =>
                new MaintenanceMiddleware(null,
                    null,
                    webHostEnv,
                    optionBuilder);

            if (expectedException == null)
            {
                testAction.ShouldNotThrow();
            }
            else
            {
                testAction.ShouldThrow(expectedException)
                    .Message.ShouldStartWith(expectedExMsgStart);
            }
        }

        [Fact]
        public void Constructor_RestoreSvcOptions()
        {
            IMaintenanceControlService svc = Substitute.For<IMaintenanceControlService, ICanRestoreState>();

            Action<MiddlewareOptionsBuilder> optionBuilder = (options) =>
            {
                // prevent other exceptions due to missing required options
                options.FillEmptyOptionsWithDefault();
            };

            Action testAction = () =>
                new MaintenanceMiddleware(null,
                    svc,
                    null,
                    optionBuilder);

            testAction.ShouldNotThrow();

            (svc as ICanRestoreState).Received(1)
                                     .RestoreState();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Invke_MaintenanceModeOn(bool isOn)
        {
            DefaultHttpContext httpContext = new DefaultHttpContext();

            bool isNextDelegateCalled = false;
            HttpContext ctx = Substitute.For<HttpContext>();
            RequestDelegate nextDelegate = (HttpContext hc) =>
            {
                isNextDelegateCalled = true;
                return Task.CompletedTask;
            };

            Action<MiddlewareOptionsBuilder> optionBuilder = (options) =>
            {
                // prevent other exceptions due to missing required options
                options.FillEmptyOptionsWithDefault();
            };

            IMaintenanceControlService svc = Substitute.For<IMaintenanceControlService>();
            svc.IsMaintenanceModeOn.Returns(isOn);

            MaintenanceMiddleware middleware = new MaintenanceMiddleware(
                nextDelegate,
                svc,

                null,
                optionBuilder);

            Action testAction = async () 
                => await middleware.Invoke(httpContext);
                

            testAction.ShouldNotThrow();
            isNextDelegateCalled.ShouldBe(!isOn);
        }
    }
}
