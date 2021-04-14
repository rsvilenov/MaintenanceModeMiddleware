using Microsoft.AspNetCore.Hosting;
using NSubstitute;
using System.IO;

namespace MaintenanceModeMiddleware.Tests.HelperTypes
{
    public static class FakeWebHostEnvironment
    {
        public static IWebHostEnvironment Create(string tempPath = null)
        {
            if (tempPath == null)
            {
                tempPath = SafeTempPath.Create(Path.GetTempPath());
            }

            IWebHostEnvironment webHostEnv = Substitute.For<IWebHostEnvironment>();

            string contentRootPath = Path.Combine(tempPath, "contentRoot");
            Directory.CreateDirectory(contentRootPath);
            webHostEnv.ContentRootPath.Returns(contentRootPath);

            string wwwRootPath = Path.Combine(tempPath, "wwwRoot");
            Directory.CreateDirectory(wwwRootPath);
            webHostEnv.WebRootPath.Returns(wwwRootPath);

            return webHostEnv;
        }
    }
}
