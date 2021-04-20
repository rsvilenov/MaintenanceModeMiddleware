using MaintenanceModeMiddleware.Configuration.Data;
using MaintenanceModeMiddleware.Configuration.Enums;
using MaintenanceModeMiddleware.Configuration.Options;
using MaintenanceModeMiddleware.Services;
using Microsoft.AspNetCore.Http;
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
    public class MiddlewareOptionsBuilder
    {
        private const int DEFAULT_503_RETRY_INTERVAL = 5300;

        private readonly OptionCollection _options;
        private readonly IPathMapperService _pathMapperSvc;
        private bool _areDefaultOptionsFilledIn;
        
        internal MiddlewareOptionsBuilder(IPathMapperService pathMapperSvc)
        {
            _options = new OptionCollection();
            _pathMapperSvc = pathMapperSvc;
        }

        /// <summary>
        /// Specify the response file which will be served to the users, trying to
        /// enter the web application while it is in maintenance mode.
        /// </summary>
        /// <param name="relativePath">File path, relative to the location, specified in the second parameter.</param>
        /// <param name="baseDir">Either ContentRootPath, or WWWRootPath.</param>
        /// <param name="code503RetryInterval">The time in seconds for the Retry-After header</param>
        /// <returns></returns>
        public MiddlewareOptionsBuilder UseResponseFromFile(string relativePath, 
            EnvDirectory baseDir, 
            int code503RetryInterval = DEFAULT_503_RETRY_INTERVAL)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                throw new ArgumentNullException(nameof(relativePath));
            }

            string envDir = _pathMapperSvc.GetPath(baseDir);
            string fullFilePath = Path.Combine(envDir, relativePath);

            if (!File.Exists(fullFilePath))
            {
                throw new FileNotFoundException($"Could not find file {relativePath}. Expected absolute path: {fullFilePath}.");
            }

            _options.Add(new ResponseFromFileOption(relativePath, baseDir, code503RetryInterval));

            return this;
        }

        /// <summary>
        /// Specify a response to be served to the users, trying to access the web application
        /// while it is in maintenance mode.
        /// </summary>
        /// <param name="response">The content of the response.</param>
        /// <param name="contentType">The type of the content: text, html or json</param>
        /// <param name="encoding">The encoding of the content</param>
        /// <param name="code503RetryInterval">The time in seconds for the Retry-After header</param>
        /// <returns></returns>
        public MiddlewareOptionsBuilder UseResponse(string response, 
            ResponseContentType contentType, 
            Encoding encoding, 
            int code503RetryInterval = DEFAULT_503_RETRY_INTERVAL)
        {
            if (string.IsNullOrEmpty(response))
            {
                throw new ArgumentNullException(nameof(response));
            }

            return UseResponse(encoding.GetBytes(response), contentType, encoding, code503RetryInterval);
        }

        /// <summary>
        /// Specify a response to be served to the users, trying to access the web application
        /// while it is in maintenance mode.
        /// </summary>
        /// <param name="responseBytes">The content of the response, encoded with the encoding, specified in the third parameter.</param>
        /// <param name="contentType">The type of the content: text, html or json</param>
        /// <param name="encoding">The encoding of the content</param>
        /// <param name="code503RetryInterval">The time in seconds for the Retry-After header</param>
        /// <returns></returns>
        public MiddlewareOptionsBuilder UseResponse(byte[] responseBytes, 
            ResponseContentType contentType, 
            Encoding encoding, 
            int code503RetryInterval = DEFAULT_503_RETRY_INTERVAL)
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

        /// <summary>
        /// Serve the built-in html response to the users, trying to access the application
        /// while it is in maintenance mode. If no other response option is specified,
        /// this is set by default.
        /// </summary>
        /// <returns></returns>
        public MiddlewareOptionsBuilder UseDefaultResponse()
        {
            _options.Add(new UseDefaultResponseOption
            {
                Value = true
            });

            return this;
        }

        /// <summary>
        /// Specify which user should retain access to the web application after
        /// it has been put in maintenance mode.
        /// </summary>
        /// <param name="userName">User name</param>
        /// <returns></returns>
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


        /// <summary>
        /// Specify which users should retain access to the web application after
        /// it has been put in maintenance mode.
        /// </summary>
        /// <param name="userNames">A collection of user names</param>
        /// <returns></returns>
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

        /// <summary>
        /// Specify which user role should retain access to the web application after
        /// it has been put in maintenance mode. 
        /// You can set user roles one by one by using this method or set multiple
        ///  roles at once by calling <see cref="BypassUserRoles">BypassUserRoles()</see>.
        /// </summary>
        /// <param name="role">Role</param>
        /// <returns></returns>
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

        /// <summary>
        /// Specify which user roles should retain access to the web application after
        /// it has been put in maintenance mode. 
        /// </summary>
        /// <param name="roles">A collection of roles.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Specify that all logged in users should have access to the entire web application
        /// after it has been put in maintanence mode.
        /// This does not apply only to the users, which had been already logged in when the application
        /// was put in maintenance mode. Since, if not specified otherwise, the Identity area remains
        /// accessible, other users may log in and still get the access to the web application.
        /// To limit the users, which should have access, use <see cref="BypassUser">BypassUser()</see>  
        /// and <see cref="BypassUserRole">BypassUserRole()</see> methods instead.
        /// </summary>
        /// <returns></returns>
        public MiddlewareOptionsBuilder BypassAllAuthenticatedUsers()
        {
            _options.Add(new BypassAllAuthenticatedUsersOption
            {
                Value = true
            }); ;

            return this;
        }

        /// <summary>
        /// Spcify which url path should remain accessible 
        /// while the web applicaiton is in maintenance mode.
        /// Pass the string with which the path BEGINS.
        /// All paths, which begin with the specified string will be matched.
        /// </summary>
        /// <param name="path">The path to be excempt from blocking.</param>
        /// <param name="comparison">How the path strings should be compared.</param>
        /// <returns></returns>
        public MiddlewareOptionsBuilder BypassUrlPath(PathString path, StringComparison comparison = StringComparison.Ordinal)
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

        /// <summary>
        /// Spcify which url paths should remain accessible 
        /// while the web applicaiton is in maintenance mode.
        /// Pass a collectin of strings with which the paths BEGIN.
        /// All paths, which begin with one of the specified string will be matched.
        /// </summary>
        /// <param name="paths">A collection of paths to be excempt from blocking.</param>
        /// <param name="comparison">How the path strings should be compared.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Specify which file extensions will still be served after the
        /// application enters maintenance mode.
        /// If this method is not called, the application will serve the following extensions:
        /// "css", "jpg", "png", "gif", "svg", "js".
        /// The static files will still be served, regardless of whether their extensions are specified here, 
        /// if <see cref="UseStaticFiles">UseStaticFiles()</see> middleware is placed before the current middleware in the chain.
        /// </summary>
        /// <param name="extension">file extension (with or without the dot)</param>
        /// <returns></returns>
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

        /// <summary>
        /// Specify which file extensions will still be served after the
        /// application enters maintenance mode.
        /// If this method is not called, the application will serve the following extensions:
        /// "css", "jpg", "png", "gif", "svg", "js".
        /// The static files will still be served, regardless of whether their extensions are specified here, 
        /// if <see cref="UseStaticFiles">UseStaticFiles()</see> middleware is placed before the current middleware in the chain.
        /// </summary>
        /// <param name="extension">A collection of file extensions (with or without the dot)</param>
        /// <returns></returns>
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

        /// <summary>
        /// Do not fill the options, which are not explicitly specified by the user,
        /// with their default values.
        /// </summary>
        /// <returns></returns>
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
                BypassUserRoles(new string[] { "Admin", "Administrator"});
            }

            if (!_options.GetAll<ResponseFromFileOption>().Any()
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

            VerifyResponseOptions();

            return _options;
        }

        private void VerifyResponseOptions()
        {
            IEnumerable<IResponseHolder> responseHolders = _options
                .GetAll<IResponseHolder>();

            if (!responseHolders.Any())
            {
                throw new ArgumentException("No response was specified.");
            }

            if (responseHolders.Count() > 1)
            {
                throw new ArgumentException("More than one response was specified.");
            }
        }
    }
}
