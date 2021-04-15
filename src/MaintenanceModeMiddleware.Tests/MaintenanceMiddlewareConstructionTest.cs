using MaintenanceModeMiddleware.Configuration.Builders;
using MaintenanceModeMiddleware.Configuration.Enums;
using Microsoft.AspNetCore.Hosting;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using Shouldly;
using System;
using System.IO;
using Xunit;


namespace MaintenanceModeMiddleware.Tests
{
    public class MaintenanceMiddlewareConstructionTest
    {
        [Fact]
        public void Constructor_DefaultOptions()
        {
            bool isBuilderCalled = false;
            Action<MiddlewareOptionsBuilder> optionBuilderDelegate = (options) =>
            {
                isBuilderCalled = true;
            };

            Action testAction = () =>
                new MaintenanceMiddleware(null,
                    null,
                    null,
                    optionBuilderDelegate);

            testAction.ShouldNotThrow();
            isBuilderCalled.ShouldBeTrue();
        }

        [Theory]
        [InlineData(true, null, null)]
        [InlineData(false, typeof(ArgumentException), "No response was specified")]
        public void Constructor_MandatoryOptions(
            bool responseSpecified,
            Type expectedException,
            string expectedExMsgStart)
        {
            Action<MiddlewareOptionsBuilder> optionBuilderDelegate = (options) =>
            {
                options.UseNoDefaultValues();

                if (responseSpecified)
                {
                    options.UseDefaultResponse();
                }
            };

            Action testAction = () =>
                new MaintenanceMiddleware(null,
                    null,
                    null,
                    optionBuilderDelegate);

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

        const string testFileNameCaseExists = "test_response_option_file_exists.txt";
        const string testFileNameCaseNotExists = "nonexistent_response_option_file.txt";
        [Theory]
        [InlineData(testFileNameCaseExists, null, null)]
        [InlineData(testFileNameCaseNotExists, typeof(FileNotFoundException), "Could not find file")]
        public void Constructor_ResponseFileOption(
            string filePath,
            Type expectedException,
            string expectedExMsgStart)
        {
            IWebHostEnvironment webHostEnv = Substitute.For<IWebHostEnvironment>();
            webHostEnv.ContentRootPath.Returns(Path.GetTempPath());
            File.Create(Path.Combine(webHostEnv.ContentRootPath, testFileNameCaseExists))
                .Dispose();

            try
            {
                Action<MiddlewareOptionsBuilder> optionBuilderDelegate = (options) =>
                {
                    options.UseResponseFromFile(filePath, PathBaseDirectory.ContentRootPath);
                // prevent other exceptions due to missing required options
                options.FillEmptyOptionsWithDefault();
                };

                Action testAction = () =>
                    new MaintenanceMiddleware(null,
                        null,
                        webHostEnv,
                        optionBuilderDelegate);

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
            finally
            {
                File.Delete(Path.Combine(webHostEnv.ContentRootPath, testFileNameCaseExists));
            }
        }

        //[Fact]
        //public void Constructor_RestoreSvcOptions()
        //{
        //    IMaintenanceControlService svc = Substitute.For<IMaintenanceControlService, ICanRestoreState>();

        //    Action<MiddlewareOptionsBuilder> optionBuilderDelegate = (options) => { };

        //    Action testAction = () =>
        //        new MaintenanceMiddleware(null,
        //            svc,
        //            null,
        //            optionBuilderDelegate);


        //    testAction.ShouldNotThrow();


        //    (svc as ICanRestoreState).Received(1)
        //                             .RestoreState();
        //}
    }
}
