using MaintenanceModeMiddleware.Configuration.Data;
using MaintenanceModeMiddleware.Configuration.Enums;
using MaintenanceModeMiddleware.Configuration.Options;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MaintenanceModeMiddleware.Configuration.Builders
{
    public class MiddlewareOptionsBuilder
    {
        internal OptionCollection Options { get; }

        internal MiddlewareOptionsBuilder()
        {
            Options = new OptionCollection();
        }

        public MiddlewareOptionsBuilder UseResponseFile(string relativePath, PathBaseDirectory baseDir)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                throw new ArgumentNullException(nameof(relativePath));
            }

            AssertResponseNotSpecified();

            string fileExtension = Path.GetExtension(relativePath);
            if (!new string[] { ".txt", ".html" }.Contains(fileExtension))
            {
                throw new ArgumentException($"The file, specified in {relativePath} must be either .txt or .html.");
            }

            FileDescriptor responseFile = new FileDescriptor(relativePath, baseDir);
            Options.Add(new ResponseFileOption
            {
                Value = responseFile
            });

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

            AssertResponseNotSpecified();

            MaintenanceResponse response 
                = new MaintenanceResponse
            {
                ContentBytes = responseBytes,
                ContentEncoding = encoding,
                ContentType = contentType
            };

            Options.Add(new ResponseOption
            {
                Value = response
            });

            return this;
        }

        public MiddlewareOptionsBuilder UseDefaultResponse()
        {
            AssertResponseNotSpecified();

            Options.Add(new UseDefaultResponseOption
            {
                Value = true
            });

            return this;
        }

        public MiddlewareOptionsBuilder Set503RetryAfterInterval(int interval)
        {
            Options.Add(new Code503RetryIntervalOption
            { 
                Value = interval
            });

            return this;
        }

        public MiddlewareOptionsBuilder BypassUser(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentNullException(nameof(userName));
            }

            Options.Add(new BypassUserNameOption
            {
                Value = userName
            });

            return this;
        }

        public MiddlewareOptionsBuilder BypassUsers(IEnumerable<string> userNames)
        {
            if (userNames == null)
            {
                throw new ArgumentNullException(nameof(userNames));
            }

            foreach (string userName in userNames)
            {
                BypassUser(userName);
            }

            return this;
        }

        public MiddlewareOptionsBuilder BypassUserRole(string role)
        {
            if (string.IsNullOrEmpty(role))
            {
                throw new ArgumentNullException(nameof(role));
            }

            Options.Add(new BypassUserRoleOption
            {
                Value = role
            });

            return this;
        }

        public MiddlewareOptionsBuilder BypassUserRoles(IEnumerable<string> roles)
        {
            foreach (string role in roles)
            {
                BypassUserRole(role);
            }

            return this;
        }

        public MiddlewareOptionsBuilder BypassAllAuthenticatedUsers()
        {
            Options.Add(new BypassAllAuthenticatedUsersOption
            {
                Value = true
            }); ;

            return this;
        }

        public MiddlewareOptionsBuilder BypassUrlPath(PathString path, StringComparison comparison = StringComparison.Ordinal)
        {
            UrlPath urlPath = 
                new UrlPath
            {
                Comparison = comparison,
                String = path
            };

            Options.Add(new BypassUrlPathOption
            {
                Value = urlPath
            });

            return this;
        }

        public MiddlewareOptionsBuilder BypassUrlPaths(IEnumerable<PathString> paths, StringComparison comparison = StringComparison.Ordinal)
        {
            foreach (PathString pathString in paths)
            {
                BypassUrlPath(pathString, comparison);
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

            Options.Add(new BypassFileExtensionOption
            {
                Value = extension
            });

            return this;
        }

        public MiddlewareOptionsBuilder BypassFileExtensions(IEnumerable<string> extensions)
        {
            foreach (string ext in extensions)
            {
                BypassFileExtension(ext);
            }

            return this;
        }

        public MiddlewareOptionsBuilder UseNoDefaultValues()
        {
            Options.Add(new UseNoDefaultValuesOption
            {
                Value = true
            });

            return this;
        }

        internal void FillEmptyOptionsWithDefault()
        {
            if (!Options.GetAll<BypassFileExtensionOption>().Any())
            {
                BypassFileExtensions(new string[] { "css", "jpg", "png", "gif", "svg", "js" });
            }

            if (!Options.GetAll<BypassUrlPathOption>().Any())
            {
                BypassUrlPath("/Identity");
            }
            
            if (!Options.GetAll<BypassUserRoleOption>().Any())
            {
                BypassUserRole("Admin");
            }

            if (!Options.GetAll<Code503RetryIntervalOption>().Any())
            {
                Set503RetryAfterInterval(5300);
            }

            if (!Options.GetAll<ResponseFileOption>().Any()
                && !Options.GetAll<ResponseOption>().Any())
            {
                UseDefaultResponse();
            }
        }

        private void AssertResponseNotSpecified()
        {
            if (Options.GetAll<ResponseOption>().Any())
            {
                throw new InvalidOperationException("You have already specified a response.");
            }

            if (Options.GetAll<ResponseFileOption>().Any())
            {
                throw new InvalidOperationException("You have already specified a response file.");
            }

            if (Options.GetAll<UseDefaultResponseOption>().Any())
            {
                throw new InvalidOperationException("You have already specified that the middleware should use its default (built-in) response.");
            }
        }
    }
}
