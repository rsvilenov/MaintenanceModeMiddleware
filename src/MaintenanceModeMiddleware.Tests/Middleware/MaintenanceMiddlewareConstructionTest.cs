using MaintenanceModeMiddleware.Configuration.Builders;
using MaintenanceModeMiddleware.Configuration.Enums;
using MaintenanceModeMiddleware.Services;
using Microsoft.AspNetCore.Hosting;
using NSubstitute;
using Shouldly;
using System;
using System.IO;
using Xunit;


namespace MaintenanceModeMiddleware.Tests
{
    public class MaintenanceMiddlewareConstructionTest
    {
        [Fact]
        public void Constructor_WithDefaultOptions_ShouldNotThrow()
        {
            Action<MiddlewareOptionsBuilder> optionBuilderDelegate = (options) => { };

            Action testAction = () =>
                new MaintenanceMiddleware(null,
                    null,
                    null,
                    optionBuilderDelegate);

            testAction.ShouldNotThrow();
        }

        [Fact]
        public void Constructor_WithDefaultOptions_ShouldCallBuilder()
        {
            bool isBuilderCalled = false;
            Action<MiddlewareOptionsBuilder> optionBuilderDelegate = (options) =>
            {
                isBuilderCalled = true;
            };

            new MaintenanceMiddleware(null,
                    null,
                    null,
                    optionBuilderDelegate);

            isBuilderCalled.ShouldBeTrue();
        }

        [Fact]
        public void Constructor_WithMissingResponseOption_ShouldThrowArgumentException()
        {
            Action<MiddlewareOptionsBuilder> optionBuilderDelegate = (options) =>
            {
                options.UseNoDefaultValues();
            };

            Action testAction = () =>
                new MaintenanceMiddleware(null,
                    null,
                    null,
                    optionBuilderDelegate);

            testAction.ShouldThrow<ArgumentException>()
                .Message.ShouldStartWith("No response was specified");
        }

        [Fact]
        public void Constructor_WithResponseFromFileOptionWhenFileExists_ShouldNotThrow()
        {
            const string testFileNameCaseExists = "test_response_option_file_exists.txt";
            string tempPath = Path.GetTempPath();
            IWebHostEnvironment webHostEnv = Substitute.For<IWebHostEnvironment>();
            webHostEnv.ContentRootPath.Returns(tempPath);
            IPathMapperService pathMappingSvc = Substitute.For<IPathMapperService>();
            pathMappingSvc.GetPath(Arg.Any<EnvDirectory>()).Returns(tempPath);

            File.Create(Path.Combine(webHostEnv.ContentRootPath, testFileNameCaseExists))
                .Dispose();

            try
            {
                Action<MiddlewareOptionsBuilder> optionBuilderDelegate = (options) =>
                {
                    options.UseResponseFromFile(testFileNameCaseExists, EnvDirectory.ContentRootPath);
                    // prevent other exceptions due to missing required options
                    options.FillEmptyOptionsWithDefault();
                };

                Action testAction = () =>
                    new MaintenanceMiddleware(null,
                        null,
                        pathMappingSvc,
                        optionBuilderDelegate);

                testAction.ShouldNotThrow();
            }
            finally
            {
                File.Delete(Path.Combine(webHostEnv.ContentRootPath, testFileNameCaseExists));
            }
        }

        [Fact]
        public void Constructor_WithResponseFromFileOptionWhenFileIsMissing_ShouldThrowFileNotFoundException()
        {
            const string testFileNameCaseNotExists = "nonexistent_response_option_file.txt";
            string tempPath = Path.GetTempPath();
            IWebHostEnvironment webHostEnv = Substitute.For<IWebHostEnvironment>();
            webHostEnv.ContentRootPath.Returns(tempPath);
            IPathMapperService pathMappingSvc = Substitute.For<IPathMapperService>();
            pathMappingSvc.GetPath(Arg.Any<EnvDirectory>()).Returns(tempPath);

            Action<MiddlewareOptionsBuilder> optionBuilderDelegate = (options) =>
            {
                options.UseResponseFromFile(testFileNameCaseNotExists, EnvDirectory.ContentRootPath);
                    // prevent other exceptions due to missing required options
                    options.FillEmptyOptionsWithDefault();
            };

            Action testAction = () =>
                new MaintenanceMiddleware(null,
                    null,
                    pathMappingSvc,
                    optionBuilderDelegate);


            testAction.ShouldThrow<FileNotFoundException>()
                .Message.ShouldStartWith("Could not find file");
        }
    }
}
