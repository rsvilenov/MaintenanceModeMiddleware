using MaintenanceModeMiddleware.Configuration.Enums;
using MaintenanceModeMiddleware.Services;
using NSubstitute;

namespace MaintenanceModeMiddleware.Tests.HelperTypes
{
    internal static class FakePathMapperService
    {
        internal static IPathMapperService Create(string tempDir = null)
        {
            var fakeEnv = FakeWebHostEnvironment.Create(tempDir);
            var fakeMapper = Substitute.For<IPathMapperService>();
            string contentRoot = fakeEnv.ContentRootPath;
            string wwwRoot = fakeEnv.WebRootPath;

            fakeMapper.GetPath(EnvDirectory.ContentRootPath)
                .Returns(contentRoot);
            fakeMapper.GetPath(EnvDirectory.WebRootPath)
                .Returns(wwwRoot);

            return fakeMapper;
        }
    }
}
