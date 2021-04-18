using MaintenanceModeMiddleware.Configuration.Data;
using MaintenanceModeMiddleware.Configuration.Enums;
using Shouldly;
using System;
using Xunit;

namespace MaintenanceModeMiddleware.Tests.Configuration
{
    public class FileDescriptorTest
    {
        [Theory]
        [InlineData("/test/file.txt")]
        [InlineData("/file.txt")]
        public void FirstConstructor_WithValidArgs_PathShouldEqualInput(string path)
        {
            var descriptor = new FileDescriptor(path);

            descriptor.Path
                .ShouldBe(path);
        }

        [Theory]
        [InlineData("test/file.txt", typeof(ArgumentException))]
        [InlineData("", typeof(ArgumentNullException))]
        [InlineData(null, typeof(ArgumentNullException))]
        public void FirstConstructor_WithInvalidArgs_ShouldThrow(string path, Type expectedException)
        {
            Action testAction = () => new FileDescriptor(path);

            testAction.ShouldThrow(expectedException);
        }


        [Theory]
        [InlineData("test/file.txt", EnvDirectory.ContentRootPath)]
        [InlineData("file.txt", EnvDirectory.WebRootPath)]
        public void SecondConstructor_WithValidArgs_ValuesShouldEqualInput(string path, EnvDirectory envDir)
        {
            var fileDescriptor = new FileDescriptor(path, envDir);

            fileDescriptor.Path
                .ShouldBe(path);
            fileDescriptor.BaseDir
                .ShouldBe(envDir);
        }

        [Theory]
        [InlineData("/test/file.txt", typeof(ArgumentException))]
        [InlineData("", typeof(ArgumentNullException))]
        [InlineData(null, typeof(ArgumentNullException))]
        public void SecondConstructor_WithInvalidPath_ShouldThrow(string path, Type expectedException)
        {
            Action testAction = () => new FileDescriptor(path, EnvDirectory.ContentRootPath);

            testAction.ShouldThrow(expectedException);
        }
    }
}
