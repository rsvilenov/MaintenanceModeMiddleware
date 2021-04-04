using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MaintenanceModeMiddleware.Configuration
{
    public class MiddlewareOptionsBuilder
    {
        private const int CODE_503_RETRY_AFTER = 5300;
        public MiddlewareOptionsBuilder UseResponseFile(string relativePath, PathBaseDirectory baseDir)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                throw new ArgumentNullException(nameof(relativePath));
            }

            string fileExtension = Path.GetExtension(relativePath);
            if (!new string[] { ".txt", ".html" }.Contains(fileExtension))
            {
                throw new ArgumentException($"The file, specified in {relativePath} must be either .txt or .html.");
            }

            ResponseFile = new FileDescriptor(relativePath, baseDir);
            return this;
        }

        public MiddlewareOptionsBuilder UseResponse(string response, ContentType contentType, Encoding encoding)
        {
            if (string.IsNullOrEmpty(response))
            {
                throw new ArgumentNullException(nameof(response));
            }

            return UseResponse(encoding.GetBytes(response), contentType, encoding);
        }

        public MiddlewareOptionsBuilder UseResponse(byte[] responseBytes, ContentType contentType, Encoding encoding)
        {
            if (responseBytes == null)
            {
                throw new ArgumentNullException(nameof(responseBytes));
            }

            Response = new MaintenanceResponse
            {
                ContentBytes = responseBytes,
                ContentEncoding = encoding,
                ContentType = contentType
            };

            return this;
        }

        public MiddlewareOptionsBuilder Set503RetryAfterInterval(int interval)
        {
            Code503RetryAfter = interval;
            return this;
        }

        public MiddlewareOptionsBuilder BypassUser(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentNullException(nameof(userName));
            }

            UserNamesToBypass.Add(userName);
            return this;
        }

        public MiddlewareOptionsBuilder BypassUsers(IEnumerable<string> userNames)
        {
            if (userNames == null)
            {
                throw new ArgumentNullException(nameof(userNames));
            }

            UserNamesToBypass.AddRange(userNames);
            return this;
        }

        public MiddlewareOptionsBuilder BypassUserRole(string role)
        {
            if (string.IsNullOrEmpty(role))
            {
                throw new ArgumentNullException(nameof(role));
            }

            UserRolesToBypass.Add(role);
            return this;
        }

        public MiddlewareOptionsBuilder BypassUserRoles(IEnumerable<string> roles)
        {
            UserRolesToBypass.AddRange(roles);
            return this;
        }

        public bool BypassAuthenticatedUsers { get; set; } = false;

        public MiddlewareOptionsBuilder BypassUrlPath(PathString path, StringComparison comparison = StringComparison.Ordinal)
        {
            UrlPathsToBypass.Add(new UrlPath
            {
                Comparison = comparison,
                String = path
            });

            return this;
        }

        public MiddlewareOptionsBuilder BypassUrlPaths(IEnumerable<PathString> paths, StringComparison comparison = StringComparison.Ordinal)
        {
            foreach (PathString pathString in paths)
            {
                UrlPathsToBypass.Add(new UrlPath
                {
                    Comparison = comparison,
                    String = pathString
                });
            }

            return this;
        }

        public MiddlewareOptionsBuilder BypassFileExtension(string extension)
        {
            if (string.IsNullOrEmpty(extension))
            {
                throw new ArgumentNullException(nameof(extension));
            }

            if (extension.StartsWith('.'))
            {
                extension = extension.Substring(1);
            }

            FileExtensionsToBypass.Add(extension);
            return this;
        }

        public MiddlewareOptionsBuilder BypassFileExtensions(IEnumerable<string> extensions)
        {
            FileExtensionsToBypass.AddRange(extensions);
            return this;
        }

        internal void FillEmptyOptionsWithDefault()
        {
            if (!FileExtensionsToBypass.Any())
            {
                BypassFileExtensions(new string[] { "css", "jpg", "png", "gif", "svg", "js" });
            }

            if (!UrlPathsToBypass.Any())
            {
                BypassUrlPath("/Identity");
            }
            
            if (!UserRolesToBypass.Any())
            {
                BypassUserRole("Admin");
            }

            if (Code503RetryAfter == 0)
            {
                Set503RetryAfterInterval(CODE_503_RETRY_AFTER);
            }

            if (Response == null && ResponseFile == null)
            {
                UseDefaultResponse = true;
            }
        }

        internal bool UseDefaultResponse { get; set; } = false;
        internal MaintenanceResponse Response { get; set; }
        internal FileDescriptor ResponseFile { get; set; }
        internal int Code503RetryAfter { get; set; }
        internal List<string> UserRolesToBypass { get; set; } = new List<string>();
        internal List<string> UserNamesToBypass { get; set; } = new List<string>();
        internal List<UrlPath> UrlPathsToBypass { get; set; } = new List<UrlPath>();
        internal List<string> FileExtensionsToBypass { get; set; } = new List<string>();
    }
}
