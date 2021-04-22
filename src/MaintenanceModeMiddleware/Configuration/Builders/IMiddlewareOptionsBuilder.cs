using MaintenanceModeMiddleware.Configuration.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaintenanceModeMiddleware.Configuration.Builders
{
    public interface IMiddlewareOptionsBuilder
    {
        /// <summary>
        /// Specify the response file which will be served to the users, trying to
        /// enter the web application while it is in maintenance mode.
        /// </summary>
        /// <param name="relativePath">File path, relative to the location, specified in the second parameter.</param>
        /// <param name="baseDir">Either ContentRootPath, or WWWRootPath.</param>
        /// <param name="code503RetryInterval">The time in seconds for the Retry-After header</param>
        /// <returns>The same <see cref="IMiddlewareOptionsBuilder"/> instance so that multiple calls can be chained.</returns>
        IMiddlewareOptionsBuilder UseResponseFromFile(string relativePath,
            EnvDirectory baseDir,
            int code503RetryInterval = DefaultValues.DEFAULT_503_RETRY_INTERVAL);

        /// <summary>
        /// Specify a response to be served to the users, trying to access the web application
        /// while it is in maintenance mode.
        /// </summary>
        /// <param name="response">The content of the response.</param>
        /// <param name="contentType">The type of the content: text, html or json</param>
        /// <param name="encoding">The encoding of the content</param>
        /// <param name="code503RetryInterval">The time in seconds for the Retry-After header</param>
        /// <returns>The same <see cref="IMiddlewareOptionsBuilder"/> instance so that multiple calls can be chained.</returns>
        IMiddlewareOptionsBuilder UseResponse(string response,
            ResponseContentType contentType,
            Encoding encoding,
            int code503RetryInterval = DefaultValues.DEFAULT_503_RETRY_INTERVAL);

        /// <summary>
        /// Specify a response to be served to the users, trying to access the web application
        /// while it is in maintenance mode.
        /// </summary>
        /// <param name="responseBytes">The content of the response, encoded with the encoding, specified in the third parameter.</param>
        /// <param name="contentType">The type of the content: text, html or json</param>
        /// <param name="encoding">The encoding of the content</param>
        /// <param name="code503RetryInterval">The time in seconds for the Retry-After header</param>
        /// <returns>The same <see cref="IMiddlewareOptionsBuilder"/> instance so that multiple calls can be chained.</returns>
        IMiddlewareOptionsBuilder UseResponse(byte[] responseBytes,
            ResponseContentType contentType,
            Encoding encoding,
            int code503RetryInterval = DefaultValues.DEFAULT_503_RETRY_INTERVAL);

        /// <summary>
        /// Serve the built-in html response to the users, trying to access the application
        /// while it is in maintenance mode. If no other response option is specified,
        /// this is set by default.
        /// </summary>
        /// <returns>The same <see cref="IMiddlewareOptionsBuilder"/> instance so that multiple calls can be chained.</returns>
        IMiddlewareOptionsBuilder UseDefaultResponse();

        /// <summary>
        /// Specify which user should retain access to the web application after
        /// it has been put in maintenance mode.
        /// </summary>
        /// <param name="userName">User name</param>
        /// <returns>The same <see cref="IMiddlewareOptionsBuilder"/> instance so that multiple calls can be chained.</returns>
        IMiddlewareOptionsBuilder BypassUser(string userName);

        /// <summary>
        /// Specify which users should retain access to the web application after
        /// it has been put in maintenance mode.
        /// </summary>
        /// <param name="userNames">A collection of user names</param>
        /// <returns>The same <see cref="IMiddlewareOptionsBuilder"/> instance so that multiple calls can be chained.</returns>
        IMiddlewareOptionsBuilder BypassUsers(IEnumerable<string> userNames);

        /// <summary>
        /// Specify which user role should retain access to the web application after
        /// it has been put in maintenance mode. 
        /// You can set user roles one by one by using this method or set multiple
        ///  roles at once by calling <see cref="BypassUserRoles">BypassUserRoles()</see>.
        /// </summary>
        /// <param name="role">Role</param>
        /// <returns>The same <see cref="IMiddlewareOptionsBuilder"/> instance so that multiple calls can be chained.</returns>
        IMiddlewareOptionsBuilder BypassUserRole(string role);

        /// <summary>
        /// Specify which user roles should retain access to the web application after
        /// it has been put in maintenance mode. 
        /// </summary>
        /// <param name="roles">A collection of roles.</param>
        /// <returns>The same <see cref="IMiddlewareOptionsBuilder"/> instance so that multiple calls can be chained.</returns>
        IMiddlewareOptionsBuilder BypassUserRoles(IEnumerable<string> roles);

        /// <summary>
        /// Specify that all logged in users should have access to the entire web application
        /// after it has been put in maintanence mode.
        /// This does not apply only to the users, which had been already logged in when the application
        /// was put in maintenance mode. Since, if not specified otherwise, the Identity area remains
        /// accessible, other users may log in and still get the access to the web application.
        /// To limit the users, which should have access, use <see cref="BypassUser">BypassUser()</see>  
        /// and <see cref="BypassUserRole">BypassUserRole()</see> methods instead.
        /// </summary>
        /// <returns>The same <see cref="IMiddlewareOptionsBuilder"/> instance so that multiple calls can be chained.</returns>
        IMiddlewareOptionsBuilder BypassAllAuthenticatedUsers();

        /// <summary>
        /// Spcify which url path should remain accessible 
        /// while the web applicaiton is in maintenance mode.
        /// Pass the string with which the path BEGINS.
        /// All paths, which begin with the specified string will be matched.
        /// </summary>
        /// <param name="path">The path to be excempt from blocking.</param>
        /// <param name="comparison">How the path strings should be compared.</param>
        /// <returns>The same <see cref="IMiddlewareOptionsBuilder"/> instance so that multiple calls can be chained.</returns>
        IMiddlewareOptionsBuilder BypassUrlPath(PathString path, StringComparison comparison = StringComparison.Ordinal);

        /// <summary>
        /// Spcify which url paths should remain accessible 
        /// while the web applicaiton is in maintenance mode.
        /// Pass a collectin of strings with which the paths BEGIN.
        /// All paths, which begin with one of the specified string will be matched.
        /// </summary>
        /// <param name="paths">A collection of paths to be excempt from blocking.</param>
        /// <param name="comparison">How the path strings should be compared.</param>
        /// <returns>The same <see cref="IMiddlewareOptionsBuilder"/> instance so that multiple calls can be chained.</returns>
        IMiddlewareOptionsBuilder BypassUrlPaths(IEnumerable<PathString> paths, StringComparison comparison = StringComparison.Ordinal);

        /// <summary>
        /// Specify which file extensions will still be served after the
        /// application enters maintenance mode.
        /// If this method is not called, the application will serve the following extensions:
        /// "css", "jpg", "png", "gif", "svg", "js".
        /// The static files will still be served, regardless of whether their extensions are specified here, 
        /// if <see cref="UseStaticFiles">UseStaticFiles()</see> middleware is placed before the current middleware in the chain.
        /// </summary>
        /// <param name="extension">file extension (with or without the dot)</param>
        /// <returns>The same <see cref="IMiddlewareOptionsBuilder"/> instance so that multiple calls can be chained.</returns>
        IMiddlewareOptionsBuilder BypassFileExtension(string extension);

        /// <summary>
        /// Specify which file extensions will still be served after the
        /// application enters maintenance mode.
        /// If this method is not called, the application will serve the following extensions:
        /// "css", "jpg", "png", "gif", "svg", "js".
        /// The static files will still be served, regardless of whether their extensions are specified here, 
        /// if <see cref="UseStaticFiles">UseStaticFiles()</see> middleware is placed before the current middleware in the chain.
        /// </summary>
        /// <param name="extension">A collection of file extensions (with or without the dot)</param>
        /// <returns>The same <see cref="IMiddlewareOptionsBuilder"/> instance so that multiple calls can be chained.</returns>
        IMiddlewareOptionsBuilder BypassFileExtensions(IEnumerable<string> extensions);

        /// <summary>
        /// Do not fill the options, which are not explicitly specified by the user,
        /// with their default values.
        /// </summary>
        /// <returns>The same <see cref="IMiddlewareOptionsBuilder"/> instance so that multiple calls can be chained.</returns>
        IMiddlewareOptionsBuilder UseNoDefaultValues();
    }
}
