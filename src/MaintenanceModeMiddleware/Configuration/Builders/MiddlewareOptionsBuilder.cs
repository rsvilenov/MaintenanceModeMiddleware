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
        private readonly OptionCollection _options;
        private bool _areDefaultOptionsFilledIn;
        
        internal MiddlewareOptionsBuilder()
        {
            _options = new OptionCollection();
        }

        public MiddlewareOptionsBuilder UseResponseFile(string relativePath, PathBaseDirectory baseDir)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                throw new ArgumentNullException(nameof(relativePath));
            }

            AssertResponseNotSpecified();

            string fileExtension = Path.GetExtension(relativePath);
            if (!new string[] { ".txt", ".html", ".json" }.Contains(fileExtension))
            {
                throw new ArgumentException($"The file, specified in {relativePath} must have one of the following extensions: .txt, .html or .json.");
            }

            FileDescriptor responseFile = new FileDescriptor(relativePath, baseDir);
            _options.Add(new ResponseFileOption
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

            _options.Add(new ResponseOption
            {
                Value = response
            });

            return this;
        }

        public MiddlewareOptionsBuilder UseDefaultResponse()
        {
            AssertResponseNotSpecified();

            _options.Add(new UseDefaultResponseOption
            {
                Value = true
            });

            return this;
        }

        public MiddlewareOptionsBuilder Set503RetryAfterInterval(int interval)
        {
            _options.Add(new Code503RetryIntervalOption
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

            _options.Add(new BypassUserNameOption
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

            if (!userNames.Any())
            {
                throw new ArgumentException($"{nameof(userNames)} is empty.");
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

            _options.Add(new BypassUserRoleOption
            {
                Value = role
            });

            return this;
        }

        public MiddlewareOptionsBuilder BypassUserRoles(IEnumerable<string> roles)
        {
            if (roles == null)
            {
                throw new ArgumentNullException(nameof(roles));
            }

            if (!roles.Any())
            {
                throw new ArgumentException($"{nameof(roles)} is empty.");
            }

            foreach (string role in roles)
            {
                BypassUserRole(role);
            }

            return this;
        }

        public MiddlewareOptionsBuilder BypassAllAuthenticatedUsers()
        {
            _options.Add(new BypassAllAuthenticatedUsersOption
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
                PathString = path
            };

            _options.Add(new BypassUrlPathOption
            {
                Value = urlPath
            });

            return this;
        }

        public MiddlewareOptionsBuilder BypassUrlPaths(IEnumerable<PathString> paths, StringComparison comparison = StringComparison.Ordinal)
        {
            if (paths == null)
            {
                throw new ArgumentNullException(nameof(paths));
            }

            if (!paths.Any())
            {
                throw new ArgumentException($"{nameof(paths)} is empty.");
            }

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

            _options.Add(new BypassFileExtensionOption
            {
                Value = extension
            });

            return this;
        }

        public MiddlewareOptionsBuilder BypassFileExtensions(IEnumerable<string> extensions)
        {
            if (extensions == null)
            {
                throw new ArgumentNullException(nameof(extensions));
            }

            if (!extensions.Any())
            {
                throw new ArgumentException($"{nameof(extensions)} is empty.");
            }

            foreach (string ext in extensions)
            {
                BypassFileExtension(ext);
            }

            return this;
        }

        public MiddlewareOptionsBuilder UseNoDefaultValues()
        {
            _options.Add(new UseNoDefaultValuesOption
            {
                Value = true
            });

            return this;
        }

        internal void FillEmptyOptionsWithDefault()
        {
            if (!_options.GetAll<BypassFileExtensionOption>().Any())
            {
                BypassFileExtensions(new string[] { "css", "jpg", "png", "gif", "svg", "js" });
            }

            if (!_options.GetAll<BypassUrlPathOption>().Any())
            {
                BypassUrlPath("/Identity");
            }
            
            if (!_options.GetAll<BypassUserRoleOption>().Any())
            {
                BypassUserRole("Admin");
            }

            if (!_options.GetAll<Code503RetryIntervalOption>().Any())
            {
                Set503RetryAfterInterval(5300);
            }

            if (!_options.GetAll<ResponseFileOption>().Any()
                && !_options.GetAll<ResponseOption>().Any())
            {
                UseDefaultResponse();
            }
        }
        internal OptionCollection GetOptions()
        {
            if (!_areDefaultOptionsFilledIn
                && _options
                    .GetSingleOrDefault<UseNoDefaultValuesOption>()
                    ?.Value != true)
            {
                FillEmptyOptionsWithDefault();
                _areDefaultOptionsFilledIn = true;
            }

            return _options;
        }

        private void AssertResponseNotSpecified()
        {
            if (_options.GetAll<ResponseOption>().Any())
            {
                throw new InvalidOperationException("You have already specified a response.");
            }

            if (_options.GetAll<ResponseFileOption>().Any())
            {
                throw new InvalidOperationException("You have already specified a response file.");
            }

            if (_options.GetAll<UseDefaultResponseOption>().Any())
            {
                throw new InvalidOperationException("You have already specified that the middleware should use its default (built-in) response.");
            }
        }
    }
}
