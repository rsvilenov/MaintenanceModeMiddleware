using MaintenanceModeMiddleware.Configuration;
using MaintenanceModeMiddleware.Configuration.Enums;
using MaintenanceModeMiddleware.Configuration.Options;
using MaintenanceModeMiddleware.Services;
using MaintenanceModeMiddleware.Tests.HelperTypes;
using Microsoft.AspNetCore.Hosting;
using NSubstitute;
using Shouldly;
using System;
using System.IO;
using Xunit;

namespace MaintenanceModeMiddleware.Tests.Configuration.Options
{
    public class ResponseFromFileOptionTest
    {
        private readonly IWebHostEnvironment _webHostEnvironment = FakeWebHostEnvironment.Create();

        [Theory]
        [InlineData("ContentRootPath;file.txt;5300", "file.txt")]
        [InlineData("ContentRootPath;file.json;5300", "file.json")]
        [InlineData("ContentRootPath;file.html;5300", "file.html")]
        public void LoadFromString_WithValidFileType_ShouldSucceed(string input, string fileName)
        {
            var option = new ResponseFromFileOption();
            string filePath = Path.Combine(_webHostEnvironment.ContentRootPath, fileName);
            File.Create(filePath);

            option.LoadFromString(input);

            option.Value.File.Path
                .ShouldEndWith(fileName);
        }

        [Theory]
        [InlineData("ContentRootPath;file.mp3;5300", "file.mp3", typeof(ArgumentException))]
        [InlineData("ContentRootPath;file;5300", "file", typeof(ArgumentException))]
        public void LoadFromString_WithInvalidFileType_ShouldThrow(string input,
            string fileName,
            Type expectedExceptionType)
        {
            var option = new ResponseFromFileOption();
            string filePath = Path.Combine(_webHostEnvironment.ContentRootPath, fileName);
            File.Create(filePath);
            Action testAction = () =>
            {
                option.LoadFromString(input);
            };


            testAction.ShouldThrow(expectedExceptionType);
        }

        [Fact]
        public void LoadFromString_WithValidInput_ValueShouldNotBeNull()
        {
            const string input = "ContentRootPath;file.txt;5300";
            var option = new ResponseFromFileOption();
            
            option.LoadFromString(input);

            option.Value.ShouldNotBeNull();
        }

        [Fact]
        public void LoadFromString_WithValidInput_StringValueShouldEqualInput()
        {
            const string input = "ContentRootPath;file.txt;5300";
            var option = new ResponseFromFileOption();
            
            option.LoadFromString(input);

            option.GetStringValue().ShouldBe(input);
        }


        [Theory]
        [InlineData("ContentRootPath", typeof(FormatException))]
        [InlineData("RootPath_NotInEnum;File.TXT;5300", typeof(ArgumentException))]
        [InlineData(null, typeof(ArgumentNullException))]
        public void LoadFromString_WithInvalidOrNullValue_ShouldThrow(string input, Type expectedException)
        {
            var option = new ResponseFromFileOption();
            Action testAction = () => option.LoadFromString(input);

            testAction.ShouldThrow(expectedException);
        }

        [Theory]
        [InlineData("ContentRootPath;file.txt;5300", "file.txt", true)]
        [InlineData("ContentRootPath;file.txt;5300", "file2.txt", false)]
        [InlineData("ContentRootPath;File.TXT;5300", "file.txt", false)]
        public void LoadFromString_WithDirrefectLetterCasing_CasingInValueShouldMatchInput(string input, string path, bool shouldBeEqual)
        {
            var option = new ResponseFromFileOption();
            
            option.LoadFromString(input);

            bool isPathEqual = option.Value.File.Path == path;
            isPathEqual.ShouldBe(shouldBeEqual);
        }

        [Theory]
        [InlineData("ContentRootPath;file.txt;5300", EnvDirectory.ContentRootPath, true)]
        [InlineData("ContentRootPath;file.txt;5300", EnvDirectory.WebRootPath, false)]
        public void LoadFromString_WithVariousEnvDirectory_EnvDirectoryInValueShouldMatchInput(string input, EnvDirectory baseDir, bool shouldBeEqual)
        {
            var option = new ResponseFromFileOption();
            
            option.LoadFromString(input);

            bool isDirEqual = option.Value.File.BaseDir == baseDir;
            isDirEqual.ShouldBe(shouldBeEqual);
        }

        [Theory]
        [InlineData("ContentRootPath;file.txt;5300", 5300)]
        [InlineData("ContentRootPath;file.txt;2200", 2200)]
        public void LoadFromString_WithValidRetryTimeout_RetryTimeoutInValueShouldMatch(string input, uint expectedTimeout)
        {
            var option = new ResponseFromFileOption();
            
            option.LoadFromString(input);

            option.Value.Code503RetryInterval
                .ShouldBe(expectedTimeout);
        }

        [Fact]
        public void LoadFromString_WithInvalidRetryTimeout_ShouldThrowArgumentException()
        {
            string input = "ContentRootPath;file.txt;abc";
            var option = new ResponseFromFileOption();
            Action testAction = () => option.LoadFromString(input);

            testAction.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void ParametrizedConstructor_WithValidParams_ValueShouldEqualParams()
        {
            string filePath = "some.txt";
            EnvDirectory baseDir = EnvDirectory.ContentRootPath;
            uint code503RetryInterval = 2000;

            ResponseFromFileOption opt =
                new ResponseFromFileOption(filePath, baseDir, code503RetryInterval, Substitute.For<IDirectoryMapperService>());

            opt.Value.ShouldNotBeNull();
            opt.Value.File.Path.ShouldBe(filePath);
            opt.Value.File.BaseDir.ShouldBe(baseDir);
            opt.Value.Code503RetryInterval.ShouldBe(code503RetryInterval);
        }

        [Fact]
        public void Postprocess_ShouldNotThrow()
        {
            IRequestHandler option = new ResponseFromFileOption();
            Action testAction = async () => await option.Postprocess(null);

            testAction.ShouldNotThrow();
        }
    }
}
