using MaintenanceModeMiddleware.Configuration;
using MaintenanceModeMiddleware.Data;
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

        public MaintenanceState GetState()
        {
            string filePath = GetFileFullPath();
            if (IO.File.Exists(filePath))
            {
                string serialized = IO.File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<MaintenanceState>(serialized);
            }

            return null;
        }

        public void SetState(MaintenanceState state)
        {
            string filePath = GetFileFullPath();

            string dirPath = IO.Path.GetDirectoryName(filePath);
            if (!IO.Directory.Exists(dirPath))
            {
                IO.Directory.CreateDirectory(dirPath);
            }

            string serialized = JsonSerializer.Serialize(state);
            IO.File.WriteAllText(filePath, serialized);
        }

        internal FileDescriptor File { get; }

        private string GetFileFullPath()
        {
            IWebHostEnvironment webHostEnv = GetDependency<IWebHostEnvironment>();

            string fullFilePath;
            if (File.BaseDir != null)
            {
                switch (File.BaseDir)
                {
                    case PathBaseDirectory.ContentRootPath:
                        fullFilePath = IO.Path.Combine(webHostEnv.ContentRootPath, File.FilePath);
                        break;
                    case PathBaseDirectory.WebRootPath:
                        fullFilePath = IO.Path.Combine(webHostEnv.ContentRootPath, File.FilePath);
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown base dir type: {File.BaseDir}.");
                }
            }
            else
            {
                fullFilePath = File.FilePath;
            }

            return fullFilePath;
        }

        private T GetDependency<T>()
        {
            return (T)((IServiceConsumer)this).ServiceProvider.GetService(typeof(T));
        }

        IServiceProvider IServiceConsumer.ServiceProvider
        {
            get; set;
        }
    }
}
