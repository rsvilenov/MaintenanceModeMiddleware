using MaintenanceModeMiddleware.Configuration.Data;
using MaintenanceModeMiddleware.Configuration.Enums;
using MaintenanceModeMiddleware.Configuration.Options;
using MaintenanceModeMiddleware.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MaintenanceModeMiddleware.Configuration.Builders
{
    /// <summary>
    /// A builder for the middleware options.
    /// </summary>
    internal class MiddlewareOptionsBuilder : IMiddlewareOptionsBuilder
    {
        private readonly OptionCollection _options;
        private readonly IDirectoryMapperService _dirMapperSvc;
        private bool _areDefaultOptionsFilledIn;
        
        internal MiddlewareOptionsBuilder(IDirectoryMapperService dirMapperSvc)
        {
            _options = new OptionCollection();
            _dirMapperSvc = dirMapperSvc;
        }

        public IMiddlewareOptionsBuilder UseResponseFromFile(string relativePath, 
            EnvDirectory baseDir, 
            int code503RetryInterval = DefaultValues.DEFAULT_503_RETRY_INTERVAL)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                throw new ArgumentNullException(nameof(relativePath));
            }

            string envDir = _dirMapperSvc.GetAbsolutePath(baseDir);
            string fullFilePath = Path.Combine(envDir, relativePath);

            if (!File.Exists(fullFilePath))
            {
                throw new FileNotFoundException($"Could not find file {relativePath}. Expected absolute path: {fullFilePath}.");
            }

            _options.Add(new ResponseFromFileOption(relativePath, baseDir, code503RetryInterval));

            return this;
        }

        public IMiddlewareOptionsBuilder UseResponse(string response, 
            ResponseContentType contentType, 
            Encoding encoding, 
            int code503RetryInterval = DefaultValues.DEFAULT_503_RETRY_INTERVAL)
        {
            if (string.IsNullOrEmpty(response))
            {
                throw new ArgumentNullException(nameof(response));
            }

            return UseResponse(encoding.GetBytes(response), contentType, encoding, code503RetryInterval);
        }

        public IMiddlewareOptionsBuilder UseResponse(byte[] responseBytes, 
            ResponseContentType contentType, 
            Encoding encoding, 
            int code503RetryInterval = DefaultValues.DEFAULT_503_RETRY_INTERVAL)
        {
            if (responseBytes == null)
            {
                throw new ArgumentNullException(nameof(responseBytes));
            }

            MaintenanceResponse response 
                = new MaintenanceResponse
                {
                    ContentBytes = responseBytes,
                    ContentEncoding = encoding,
                    ContentType = contentType,
                    Code503RetryInterval = code503RetryInterval
                };

            _options.Add(new ResponseOption
            {
                Value = response
            });

            return this;
        }

        public IMiddlewareOptionsBuilder UseDefaultResponse()
        {
            _options.Add(new UseDefaultResponseOption
            {
                Value = true
            });

            return this;
        }

        public IMiddlewareOptionsBuilder UseRedirect(PathString redirectPath)
        {
            if (!redirectPath.HasValue)
            {
                throw new ArgumentNullException($"{nameof(redirectPath)} is empty.");
            }

            _options.Add(new UseRedirectOption
            {
                Value = redirectPath
            });

            return this;
        }

        public IMiddlewareOptionsBuilder BypassUser(string userName)
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

        public IMiddlewareOptionsBuilder BypassUsers(IEnumerable<string> userNames)
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

        public IMiddlewareOptionsBuilder BypassUserRole(string role)
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

        public IMiddlewareOptionsBuilder BypassUserRoles(IEnumerable<string> roles)
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

        public IMiddlewareOptionsBuilder BypassAllAuthenticatedUsers()
        {
            _options.Add(new BypassAllAuthenticatedUsersOption
            {
                Value = true
            }); ;

            return this;
        }

        public IMiddlewareOptionsBuilder BypassUrlPath(PathString path, StringComparison comparison = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(path.Value))
            {
                throw new ArgumentException($"The path specified in argument {nameof(path)} is null or empty.");
            }

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

        public IMiddlewareOptionsBuilder BypassUrlPaths(IEnumerable<PathString> paths, StringComparison comparison = StringComparison.Ordinal)
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

        public IMiddlewareOptionsBuilder BypassFileExtension(string extension)
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

        public IMiddlewareOptionsBuilder BypassFileExtensions(IEnumerable<string> extensions)
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

        public IMiddlewareOptionsBuilder UseNoDefaultValues()
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
                BypassUserRoles(new string[] { "Admin", "Administrator"});
            }

            if (!_options.GetAll<ResponseFromFileOption>().Any()
                && !_options.GetAll<ResponseOption>().Any()
                && !_options.GetAll<IRedirectInitializer>().Any())
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

            VerifyResponseOptions();

            return _options;
        }

        private void VerifyResponseOptions()
        {
            IEnumerable<IResponseHolder> responseHolders = _options
                .GetAll<IResponseHolder>();

            IEnumerable<IRedirectInitializer> redirectInitializers = _options
                .GetAll<IRedirectInitializer>();

            if (!responseHolders.Any() && ! redirectInitializers.Any())
            {
                throw new ArgumentException("No response or redirect was specified.");
            }

            if (responseHolders.Any() && redirectInitializers.Any())
            {
                throw new ArgumentException("Both a response and a redirect were specified.");
            }

            if (responseHolders.Count() > 1)
            {
                throw new ArgumentException("More than one response was specified.");
            }
        }
    }
}
