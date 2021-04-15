using MaintenanceModeMiddleware.Configuration.Data;
using MaintenanceModeMiddleware.Configuration.Enums;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;

namespace MaintenanceModeMiddleware.Configuration.Options
{
    internal class ResponseFromFileOption : Option<FileMaintenanceResponse>, IResponseHolder
    {
        private const char PARTS_SEPARATOR = ';';

        internal ResponseFromFileOption() { }

        internal ResponseFromFileOption(string filePath, PathBaseDirectory baseDir, int code503RetryInterval)
        {
            SetValue(filePath, baseDir, code503RetryInterval);
        }

        public override void LoadFromString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentNullException(nameof(str));
            }

            string[] parts = str.Split(PARTS_SEPARATOR);
            if (parts.Length != 3)
            {
                throw new FormatException($"{nameof(str)} is in incorrect format.");
            }

            if (!Enum.TryParse(parts[0], out PathBaseDirectory baseDir))
            {
                throw new ArgumentException($"Unknown base directory type {parts[0]}");
            }

            if (!int.TryParse(parts[2], out int code503RetryInterval))
            {
                throw new ArgumentException("Unable to parse the code 503 retry interval.");
            }

            SetValue(parts[1], baseDir, code503RetryInterval);
        }

        private void SetValue(string filePath, PathBaseDirectory baseDir, int code503RetryInterval)
        {
            Value = new FileMaintenanceResponse
            {
                File = new FileDescriptor(filePath, baseDir),
                Code503RetryInterval = code503RetryInterval
            };
        }

        public override string GetStringValue()
        {
            return $"{Value.File.BaseDir}{PARTS_SEPARATOR}{Value.File.Path}{PARTS_SEPARATOR}{Value.Code503RetryInterval}";
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
                    ContentType = contentType,
                    Code503RetryInterval = Value.Code503RetryInterval
                };
            }
        }

        public override void Verify(IWebHostEnvironment webHostEnv)
        {
            string fullPath = GetFileFullPath(webHostEnv);
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"Could not find file {Value.File.Path}. Expected absolute path: {fullPath}.");
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
            string baseDir = Value.File.BaseDir == PathBaseDirectory.WebRootPath
                               ? webHostEnv.WebRootPath
                               : webHostEnv.ContentRootPath;

            string absPath = Path.Combine(baseDir, Value.File.Path);
            return absPath;
        }
    }
}
