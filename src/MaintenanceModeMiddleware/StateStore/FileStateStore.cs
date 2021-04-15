using MaintenanceModeMiddleware.Configuration.Data;
using MaintenanceModeMiddleware.Configuration.Enums;
using MaintenanceModeMiddleware.Configuration.State;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Text.Json;
using IO = System.IO;

namespace MaintenanceModeMiddleware.StateStore
{
    internal class FileStateStore : IStateStore, IServiceConsumer
    {
        internal FileStateStore(FileDescriptor file)
        {
            File = file;
        }

        public StorableMaintenanceState GetState()
        {
            string filePath = GetFileFullPath();
            if (!IO.File.Exists(filePath))
            {
                return null;
            }

            string serialized = IO.File.ReadAllText(filePath);
            if (string.IsNullOrEmpty(serialized))
            {
                return null;
            }

            return JsonSerializer.Deserialize<StorableMaintenanceState>(serialized);
        }

        public void SetState(StorableMaintenanceState state)
        {
            string filePath = GetFileFullPath();

            string dirPath = IO.Path.GetDirectoryName(filePath);
            if (!IO.Directory.Exists(dirPath))
            {
                IO.Directory.CreateDirectory(dirPath);
            }

            string serialized = JsonSerializer.Serialize(state, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            IO.File.WriteAllText(filePath, serialized);
        }

        internal FileDescriptor File { get; }

        private string GetFileFullPath()
        {
            IWebHostEnvironment webHostEnv = GetWebHostEnvironment();

            if (File.BaseDir == null)
            {
                return File.Path;
            }

            string fullFilePath;
            switch (File.BaseDir)
            {
                case PathBaseDirectory.ContentRootPath:
                    fullFilePath = IO.Path.Combine(webHostEnv.ContentRootPath, File.Path);
                    break;
                case PathBaseDirectory.WebRootPath:
                    fullFilePath = IO.Path.Combine(webHostEnv.WebRootPath, File.Path);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown base dir type: {File.BaseDir}.");
            }
            
            return fullFilePath;
        }

        private IWebHostEnvironment GetWebHostEnvironment()
        {
            IServiceProvider svcProvider = ((IServiceConsumer)this).ServiceProvider;
            return (IWebHostEnvironment)svcProvider.GetService(typeof(IWebHostEnvironment));
        }

        IServiceProvider IServiceConsumer.ServiceProvider
        {
            get; set;
        }
    }
}
