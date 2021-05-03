using MaintenanceModeMiddleware.Configuration.Enums;
using MaintenanceModeMiddleware.Services;
using NSubstitute;

namespace MaintenanceModeMiddleware.Tests.HelperTypes
{
    internal static class FakeDirectoryMapperService
    {
        internal static IDirectoryMapperService Create(string tempDir = null)
        {
            var fakeEnv = FakeWebHostEnvironment.Create(tempDir);
            var fakeMapper = Substitute.For<IDirectoryMapperService>();
            string contentRoot = fakeEnv.ContentRootPath;
            string wwwRoot = fakeEnv.WebRootPath;

            fakeMapper.GetAbsolutePath(EnvDirectory.ContentRootPath)
                .Returns(contentRoot);
            fakeMapper.GetAbsolutePath(EnvDirectory.WebRootPath)
                .Returns(wwwRoot);

            return fakeMapper;
        }
    }
}
