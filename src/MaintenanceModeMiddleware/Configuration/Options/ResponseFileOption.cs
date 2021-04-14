using MaintenanceModeMiddleware.Configuration.Data;
using MaintenanceModeMiddleware.Configuration.Enums;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;

namespace MaintenanceModeMiddleware.Configuration.Options
{
    internal class ResponseFileOption : Option<FileDescriptor>, IResponseHolder
    {
        internal const char PARTS_SEPARATOR = ';';

        public override void LoadFromString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentNullException(nameof(str));
            }

            string[] parts = str.Split(PARTS_SEPARATOR);
            if (parts.Length != 2)
            {
                throw new FormatException($"{nameof(str)} is in incorrect format.");
            }

            if (!Enum.TryParse(parts[0], out PathBaseDirectory baseDir))
            {
                throw new ArgumentException($"Unknown base directory type {parts[0]}");
            }

            Value = new FileDescriptor(parts[1], baseDir);
        }

        public override string GetStringValue()
        {
            return $"{Value.BaseDir}{PARTS_SEPARATOR}{Value.FilePath}";
        }

        public MaintenanceResponse GetResponse(IWebHostEnvironment webHostEnv)
        {
            string fullPath = GetFileFullPath(webHostEnv);

            using (StreamReader sr = new StreamReader(fullPath,
                detectEncodingFromByteOrderMarks: true))
            {
                ContentType contentType = GetContentType(fullPath);

                return new MaintenanceResponse
                {
                    ContentBytes = sr.CurrentEncoding.GetBytes(sr.ReadToEnd()),
                    ContentEncoding = sr.CurrentEncoding,
                    ContentType = contentType
                };
            }
        }

        public override void Verify(IWebHostEnvironment webHostEnv)
        {
            string fullPath = GetFileFullPath(webHostEnv);
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"Could not find file {Value.FilePath}. Expected absolute path: {fullPath}.");
            }

            // should not throw
            GetContentType(fullPath);
        }
        private ContentType GetContentType(string fullPath)
        {
            return Path.GetExtension(fullPath) switch
            {
                ".txt" => ContentType.Text,
                ".html" => ContentType.Html,
                ".json" => ContentType.Json,
                _ => throw new InvalidOperationException($"Path {fullPath} is not in any of the supported formats."),
            };
        }

        private string GetFileFullPath(IWebHostEnvironment webHostEnv)
        {
            string baseDir = Value.BaseDir == PathBaseDirectory.WebRootPath
                               ? webHostEnv.WebRootPath
                               : webHostEnv.ContentRootPath;

            string absPath = Path.Combine(baseDir, Value.FilePath);
            return absPath;
        }
    }
}
