using MaintenanceModeMiddleware.Configuration.Enums;
using MaintenanceModeMiddleware.Services;
using MaintenanceModeMiddleware.Tests.HelperTypes;
using Microsoft.AspNetCore.Hosting;
using Shouldly;
using System;
using Xunit;

namespace MaintenanceModeMiddleware.Tests.Services
{
    public class DirectoryMapperServiceTest
    {
        private readonly IDirectoryMapperService _mapperSvc;
        
        public DirectoryMapperServiceTest()
        {
            IWebHostEnvironment webHostEnv = FakeWebHostEnvironment.Create();
            _mapperSvc = new DirectoryMapperService(webHostEnv);
        }

        [Theory]
        [InlineData(EnvDirectory.ContentRootPath)]
        [InlineData(EnvDirectory.WebRootPath)]
        public void GetAbsolutePath_WithValidEnumValue_ShouldReturnPath(EnvDirectory dir)
        {
            Func<string> testFunc = () => _mapperSvc.GetAbsolutePath(dir);

            string path = testFunc.ShouldNotThrow();

            path.ShouldNotBeNullOrEmpty();
        }

        [Fact]
        public void GetAbsolutePath_WithInvalidEnumValue_ShouldThrow()
        {
            EnvDirectory invalidDir = (EnvDirectory)(-1);

            Action testAction = () => _mapperSvc.GetAbsolutePath(invalidDir);

            testAction.ShouldThrow<InvalidOperationException>();
        }
    }
}
