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
            uint code503RetryInterval = DefaultValues.DEFAULT_503_RETRY_INTERVAL)
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
            uint code503RetryInterval = DefaultValues.DEFAULT_503_RETRY_INTERVAL)
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
            uint code503RetryInterval = DefaultValues.DEFAULT_503_RETRY_INTERVAL)
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
            _options.Add(new DefaultResponseOption
            {
                Value = true
            });

            return this;
        }


        public IMiddlewareOptionsBuilder UsePathRedirect(PathString path, Action<IPathRedirectOptionsBulder> options = null)
        {
            if (!path.HasValue)
            {
                throw new ArgumentNullException($"{nameof(path)} is empty.");
            }

            PathRedirectOptionsBulder builder = new PathRedirectOptionsBulder();
            options?.Invoke(builder);

            PathRedirectData data = builder.GetPathRedirectData();
            data.Path = path;

            _options.Add(new PathRedirectOption
            {
                Value = data
            });

            return this;
        }

        public IMiddlewareOptionsBuilder UseRedirect(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            if (!Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
            {
                throw new ArgumentException("The passed url is not well formatted.", paramName: nameof(url));
            }

            _options.Add(new RedirectOption
            {
                Value = url
            });

            return this;
        }


        public IMiddlewareOptionsBuilder UseControllerAction(string controllerName, string actionName, string areaName = null)
        {
            if (string.IsNullOrEmpty(controllerName))
            {
                throw new ArgumentNullException(nameof(controllerName));
            }

            if (string.IsNullOrEmpty(actionName))
            {
                throw new ArgumentNullException(nameof(actionName));
            }

            string controllerSuffix = "Controller";
            if (controllerName.EndsWith(controllerSuffix))
            {
                controllerName = controllerName.Remove(controllerName.Length - controllerSuffix.Length);
            }

            _options.Add(new ControllerActionOption
            {
                Value = new ControllerActionData
                {
                    ControllerName = controllerName,
                    ActionName = actionName,
                    AreaName = areaName ?? string.Empty
                }
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
            _options.Add(new NoDefaultValuesOption
            {
                Value = true
            });

            return this;
        }

        internal void FillEmptyOptionsWithDefault()
        {
            if (!_options.Any<BypassFileExtensionOption>())
            {
                BypassFileExtensions(new string[] { "css", "jpg", "png", "gif", "svg", "js" });
            }

            if (!_options.Any<BypassUrlPathOption>())
            {
                BypassUrlPath("/Identity");
            }
            
            if (!_options.Any<BypassUserRoleOption>())
            {
                BypassUserRoles(new string[] { "Admin", "Administrator"});
            }

            if (!_options.Any<IResponseHolder>()
                && !_options.Any<IRedirectInitializer>()
                && !_options.Any<IRouteDataModifier>())
            {
                UseDefaultResponse();
            }
        }
        internal OptionCollection GetOptions()
        {
            if (!_areDefaultOptionsFilledIn
                && _options
                    .GetSingleOrDefault<NoDefaultValuesOption>()
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
            IEnumerable<IOption> responseOptions = _options
                .GetAll<IResponseHolder>()
                .Cast<IOption>()
                .Concat(_options.GetAll<IRedirectInitializer>())
                .Concat(_options.GetAll<IRouteDataModifier>());


            if (!responseOptions.Any())
            {
                throw new ArgumentException("No response, redirect or route data was specified.");
            }

            if (responseOptions.Count() > 1)
            {
                throw new ArgumentException("More than one response, redirect or route data was specified.");
            }
        }
    }
}
