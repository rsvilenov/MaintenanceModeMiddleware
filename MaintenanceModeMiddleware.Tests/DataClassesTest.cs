using MaintenanceModeMiddleware.Configuration.Data;
using MaintenanceModeMiddleware.Configuration.Enums;
using Shouldly;
using System;
using Xunit;

namespace MaintenanceModeMiddleware.Tests
{
    public class DataClassesTest
    {
        [Theory]
        [InlineData("/test/file.txt", null)]
        [InlineData("test/file.txt", typeof(ArgumentException))]
        [InlineData("", typeof(ArgumentNullException))]
        [InlineData(null, typeof(ArgumentNullException))]
        public void FileDescriptor_FirstConstructor(string path, Type expectedException)
        {
            Action testAction = () => new FileDescriptor(path);

            if (expectedException != null)
            {
                testAction.ShouldThrow(expectedException);
            }
            else
            {
                testAction.ShouldNotThrow();
            }
        }

        [Theory]
        [InlineData("test/file.txt", null)]
        [InlineData("/test/file.txt", typeof(ArgumentException))]
        [InlineData("", typeof(ArgumentNullException))]
        [InlineData(null, typeof(ArgumentNullException))]
        public void FileDescriptor_SecondConstructor(string path, Type expectedException)
        {
            Action testAction = () => new FileDescriptor(path, PathBaseDirectory.ContentRootPath);

            if (expectedException != null)
            {
                testAction.ShouldThrow(expectedException);
            }
            else
            {
                testAction.ShouldNotThrow();
            }
        }

        [Theory]
        [InlineData("/test/file.txt", "/test/file.txt", null, true)]
        [InlineData("/test/file.txt", "/test/npFile.txt", null, false)]
        [InlineData("test/file.txt", "test/file.txt", PathBaseDirectory.ContentRootPath, true)]
        [InlineData("test/file.txt", "test/noFile.txt", PathBaseDirectory.WebRootPath, false)]
        public void FileDescriptor_Path(string path, string result, PathBaseDirectory? baseDir, bool shouldPathBeEqual)
        {
            FileDescriptor descriptor = null;
            Action testAction = () => descriptor = baseDir.HasValue 
                ? new FileDescriptor(path, baseDir.Value)
                : new FileDescriptor(path);

            testAction.ShouldNotThrow();
            descriptor.BaseDir.ShouldBe(baseDir);

            if (shouldPathBeEqual)
            {
                descriptor.FilePath.ShouldBe(result);
            }
            else
            {
                descriptor.FilePath.ShouldNotBe(result);
            }
        }

        [Theory]
        [InlineData(ContentType.Html, "text/html")]
        [InlineData(ContentType.Text, "text/plain")]
        public void MaintenanceResponse_GetContentType(ContentType contentType, string contnetTypeString)
        {
            MaintenanceResponse response = new MaintenanceResponse
            {
                ContentType = contentType
            };
            Func<string> testFunc = () => response.GetContentTypeString();

            testFunc.ShouldNotThrow().ShouldBe(contnetTypeString);
        }
    }
}
