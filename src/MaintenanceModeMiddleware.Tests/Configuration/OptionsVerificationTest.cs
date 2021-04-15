using MaintenanceModeMiddleware.Configuration.Options;
using MaintenanceModeMiddleware.Tests.HelperTypes;
using Microsoft.AspNetCore.Hosting;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace MaintenanceModeMiddleware.Tests.Configuration
{
    public class OptionsVerificationTest
    {
        private readonly IWebHostEnvironment _webHostEnvironment = FakeWebHostEnvironment.Create();

        [Theory]
        [InlineData("ContentRootPath;file.txt;5300", true, null)]
        [InlineData("ContentRootPath;file.json;5300", true, null)]
        [InlineData("ContentRootPath;file.html;5300", true, null)]
        [InlineData("ContentRootPath;file.html;5300", false, typeof(FileNotFoundException))]
        [InlineData("ContentRootPath;file.mp3;5300", true, typeof(InvalidOperationException))]
        public void Test_ResponseFileOption_Verify(string input, bool createFile, Type expectedExceptionType)
        {
            var option = new ResponseFromFileOption();
            
            Action testAction = () =>
            {
                option.LoadFromString(input);

                if (createFile)
                {
                    string filePath = Path.Combine(_webHostEnvironment.ContentRootPath, option.Value.File.Path);
                    File.Create(filePath);
                }

                option.Verify(_webHostEnvironment);
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
