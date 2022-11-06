using MaintenanceModeMiddleware.Configuration.Data;
using MaintenanceModeMiddleware.Configuration.Enums;
using MaintenanceModeMiddleware.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MaintenanceModeMiddleware.Configuration.Options
{
    internal class ResponseFromFileOption : Option<FileMaintenanceResponse>, IRequestHandler
    {
        private const char PARTS_SEPARATOR = ';';

        internal ResponseFromFileOption() { }

        private readonly IDirectoryMapperService _dirMapperSvc;

        internal ResponseFromFileOption(string filePath, EnvDirectory baseDir, uint code503RetryInterval, IDirectoryMapperService dirMapperSvc)
        {
            _dirMapperSvc = dirMapperSvc;
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

            if (!Enum.TryParse(parts[0], out EnvDirectory baseDir))
            {
                throw new ArgumentException($"Unknown base directory type {parts[0]}");
            }

            if (!uint.TryParse(parts[2], out uint code503RetryInterval))
            {
                throw new ArgumentException("Unable to parse the code 503 retry interval.");
            }

            SetValue(parts[1], baseDir, code503RetryInterval);
        }

        private void SetValue(string filePath, EnvDirectory baseDir, uint code503RetryInterval)
        {
            if (!TryGetContentType(filePath, out _))
            {
                string fileName = Path.GetFileName(filePath);
                throw new ArgumentException($"The file, specified in {filePath} must have one of the following extensions: .txt, .html or .json.");
            }

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

        private MaintenanceResponse GetResponse()
        {
            string fullPath = GetFileFullPath(_dirMapperSvc);

            using (StreamReader sr = new StreamReader(fullPath,
                detectEncodingFromByteOrderMarks: true))
            {
                TryGetContentType(fullPath, out ResponseContentType? contentType);

                return new MaintenanceResponse
                {
                    ContentBytes = sr.CurrentEncoding.GetBytes(sr.ReadToEnd()),
                    ContentEncoding = sr.CurrentEncoding,
                    ContentType = contentType.Value,
                    Code503RetryInterval = Value.Code503RetryInterval
                };
            }
        }

        private bool TryGetContentType(string filePath, out ResponseContentType? contentType)
        {
            string extension = Path.GetExtension(filePath)
                .ToLower();

            contentType = extension switch
            {
                ".txt" => ResponseContentType.Text,
                ".html" => ResponseContentType.Html,
                ".json" => ResponseContentType.Json,
                _ => null,
            };

            return contentType.HasValue;
        }

        private string GetFileFullPath(IDirectoryMapperService dirMapperSvc)
        {
            string envDir = dirMapperSvc.GetAbsolutePath(Value.File.BaseDir.Value);

            string absPath = Path.Combine(envDir, Value.File.Path);
            return absPath;
        }


        Task IRequestHandler.Postprocess(HttpContext context)
        {
            return Task.CompletedTask;
        }

        async Task<PreprocessResult> IRequestHandler.Preprocess(HttpContext context)
        {
            MaintenanceResponse response = GetResponse();

            context
                .Response
                .StatusCode = StatusCodes.Status503ServiceUnavailable;

            context
                .Response
                .Headers
                .Add("Retry-After", response.Code503RetryInterval.ToString());

            context
                .Response
                .ContentType = response.GetContentTypeString();

            string responseStr = response
                .ContentEncoding
                .GetString(response.ContentBytes);

            await context
                .Response
                .WriteAsync(responseStr,
                    response.ContentEncoding);

            return new PreprocessResult { CallNext = false };
        }
    }
}
