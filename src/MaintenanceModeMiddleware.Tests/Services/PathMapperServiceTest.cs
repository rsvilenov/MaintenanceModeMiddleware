using MaintenanceModeMiddleware.Configuration.Enums;
using MaintenanceModeMiddleware.Services;
using MaintenanceModeMiddleware.Tests.HelperTypes;
using Microsoft.AspNetCore.Hosting;
using Shouldly;
using System;
using Xunit;

namespace MaintenanceModeMiddleware.Tests.Services
{
    public class PathMapperServiceTest
    {
        private readonly IWebHostEnvironment _webHostEnv;
        private readonly IPathMapperService _mapperSvc;
        public PathMapperServiceTest()
        {
            _webHostEnv = FakeWebHostEnvironment.Create();
            _mapperSvc = new PathMapperService(_webHostEnv);
        }

        [Theory]
        [InlineData(EnvDirectory.ContentRootPath)]
        [InlineData(EnvDirectory.WebRootPath)]
        public void GetPath_WithValidEnumValue_ShouldReturnPath(EnvDirectory dir)
        {
            Func<string> testFunc = () => _mapperSvc.GetPath(dir);

            string path = testFunc.ShouldNotThrow();

            path.ShouldNotBeNullOrEmpty();
        }

        [Fact]
        public void GetPath_WithInvalidEnumValue_ShouldThrow()
        {
            EnvDirectory invalidDir = (EnvDirectory)(-1);

            Action testAction = () => _mapperSvc.GetPath(invalidDir);

            testAction.ShouldThrow<InvalidOperationException>();
        }
    }
}
