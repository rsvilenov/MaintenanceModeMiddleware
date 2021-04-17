using MaintenanceModeMiddleware.Configuration.Data;
using MaintenanceModeMiddleware.Configuration.State;
using MaintenanceModeMiddleware.Services;
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
            if (File.BaseDir == null)
            {
                return File.Path;
            }

            IPathMapperService pathMapperSvc = GetPathMapperService();

            string envPath = pathMapperSvc.GetPath(File.BaseDir.Value);

            return IO.Path.Combine(envPath, File.Path);
        }

        private IPathMapperService GetPathMapperService()
        {
            IServiceProvider svcProvider = ((IServiceConsumer)this).ServiceProvider;
            return (IPathMapperService)svcProvider.GetService(typeof(IPathMapperService));
        }

        IServiceProvider IServiceConsumer.ServiceProvider
        {
            get; set;
        }
    }
}
