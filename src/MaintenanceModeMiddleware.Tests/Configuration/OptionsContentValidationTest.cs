using MaintenanceModeMiddleware.Configuration.Options;
using MaintenanceModeMiddleware.Tests.HelperTypes;
using Microsoft.AspNetCore.Hosting;
using Shouldly;
using System;
using System.IO;
using Xunit;

namespace MaintenanceModeMiddleware.Tests.Configuration
{
    public class OptionsContentValidationTest
    {
        private readonly IWebHostEnvironment _webHostEnvironment = FakeWebHostEnvironment.Create();

        [Theory]
        [InlineData("ContentRootPath;file.txt;5300", "file.txt", true, null)]
        [InlineData("ContentRootPath;file.json;5300", "file.json", true, null)]
        [InlineData("ContentRootPath;file.html;5300", "file.html", true, null)]
        [InlineData("ContentRootPath;file.mp3;5300", "file.mp3", true, typeof(ArgumentException))]
        public void Test_ResponseFileOption_ValidateFile(string input, 
            string fileName,
            bool createFile, 
            Type expectedExceptionType)
        {
            var option = new ResponseFromFileOption();
            
            Action testAction = () =>
            {
                if (createFile)
                {
                    string filePath = Path.Combine(_webHostEnvironment.ContentRootPath, fileName);
                    File.Create(filePath);
                }

                option.LoadFromString(input);
            };

            if (expectedExceptionType != null)
            {
                testAction.ShouldThrow(expectedExceptionType);
            }
            else
            {
                testAction.ShouldNotThrow();
            }
        }
    }
}
