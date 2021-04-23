using MaintenanceModeMiddleware.Configuration.Data;
using MaintenanceModeMiddleware.Configuration.Enums;
using MaintenanceModeMiddleware.Configuration.State;
using MaintenanceModeMiddleware.Services;
using System.Text.Json;
using IO = System.IO;

namespace MaintenanceModeMiddleware.StateStore
{
    internal class FileStateStore : IStateStore
    {
        private readonly IPathMapperService _pathMapperSvc;
        public FileStateStore(IPathMapperService pathMapperService)
        {
            _pathMapperSvc = pathMapperService;

            File = new FileDescriptor("maintenanceState.json",
                   EnvDirectory.ContentRootPath);
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

            string serialized = JsonSerializer.Serialize(state, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            IO.File.WriteAllText(filePath, serialized);
        }

        internal FileDescriptor File { get; }

        private string GetFileFullPath()
        {
            string envPath = _pathMapperSvc.GetPath(File.BaseDir.Value);

            return IO.Path.Combine(envPath, File.Path);
        }
    }
}
