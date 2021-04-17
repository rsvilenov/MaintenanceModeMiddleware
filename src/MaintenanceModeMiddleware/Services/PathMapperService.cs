using MaintenanceModeMiddleware.Configuration.Enums;
using Microsoft.AspNetCore.Hosting;
using System;

namespace MaintenanceModeMiddleware.Services
{
    internal class PathMapperService : IPathMapperService
    {
        private readonly IWebHostEnvironment _webHostEnv;

        public PathMapperService(IWebHostEnvironment webHostEnv)
        {
            _webHostEnv = webHostEnv;
        }

        public string GetPath(EnvDirectory dir)
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
