using MaintenanceModeMiddleware.Configuration.Enums;
using Microsoft.AspNetCore.Hosting;
using System;

namespace MaintenanceModeMiddleware.Services
{
    internal class DirectoryMapperService : IDirectoryMapperService
    {
        private readonly IWebHostEnvironment _webHostEnv;

        public DirectoryMapperService(IWebHostEnvironment webHostEnv)
        {
            _webHostEnv = webHostEnv;
        }

        public string GetAbsolutePath(EnvDirectory dir)
        {
            return dir switch
            {
                EnvDirectory.ContentRootPath => _webHostEnv.ContentRootPath,
                EnvDirectory.WebRootPath => _webHostEnv.WebRootPath,
                _ => throw new InvalidOperationException($"Unknown env dir type: {dir}."),
            };
        }
    }
}
