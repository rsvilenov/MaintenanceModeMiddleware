using MaintenanceModeMiddleware.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MaintenanceModeMiddleware
{
    public class MaintenanceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMaintenanceControlService _maintenanceCtrlSev;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly Options _options;
        private readonly MaintenanceResponse _response;

        public MaintenanceMiddleware(RequestDelegate next,
            IMaintenanceControlService maintenanceCtrlSev,
            IWebHostEnvironment webHostEnvironment,
            Action<Options> options)
        {
            _next = next;
            _maintenanceCtrlSev = maintenanceCtrlSev;
            _webHostEnvironment = webHostEnvironment;

            _options = new Options();
            options?.Invoke(_options);

            VerifyOptions();

            _response = GetMaintenanceResponse();

            // We should try to restore the state after all dependecies have been registered,
            // because some implementation of IStateStore may rely on a dependency, such is the case
            // with FileStateStore - it relies on a resolvable instance of IWebHostEnvironment.
            // That's why we are doing this here and not, for example, in the service's constructor.
            if (_maintenanceCtrlSev is ICanRestoreState iCanRestoreState)
            {
                iCanRestoreState.RestoreState();
            }
        }

        private MaintenanceResponse GetMaintenanceResponse()
        {
            if (_options.Response != null)
            {
                return _options.Response;
            }

            string absPath = GetAbsolutePathOfResponseFile();
            using StreamReader sr = new StreamReader(absPath, detectEncodingFromByteOrderMarks: true);

            return new MaintenanceResponse
            {
                ContentBytes = sr.CurrentEncoding.GetBytes(sr.ReadToEnd()),
                ContentEncoding = sr.CurrentEncoding,
                ContentType = absPath.EndsWith(".txt") 
                    ? "text/plain" : "text/html"
            };
        }

        private void VerifyOptions()
        {
            if (_options.Response == null)
            {
                if (_options.ResponseFile.FilePath == null)
                {
                    throw new ArgumentNullException($"No value for {nameof(_options.Response)}, nor for {nameof(_options.ResponseFile)} was specified.");
                }

                if (_options.ResponseFile.FilePath != null)
                {
                    string absPath = GetAbsolutePathOfResponseFile();

                    if (!System.IO.File.Exists(absPath))
                    {
                        throw new ArgumentException($"Could not find file {_options.ResponseFile.FilePath}, specified for option {nameof(_options.ResponseFile)}. Expected absolute path: {absPath}.");
                    }

                    string fileExtension = Path.GetExtension(absPath);
                    if (!new string[] { ".txt", ".html" }.Contains(fileExtension))
                    {
                        throw new ArgumentException($"The file, specified in {absPath} must be either .txt or .html.");
                    }
                }
            }
        }

        private string GetAbsolutePathOfResponseFile()
        {
            if (_options.ResponseFile.BaseDir == null)
            {
                return _options.ResponseFile.FilePath;
            }

            string baseDir = _options.ResponseFile.BaseDir == PathBaseDirectory.WebRootPath 
                ? _webHostEnvironment.WebRootPath 
                : _webHostEnvironment.ContentRootPath;
            
            return Path.Combine(baseDir, _options.ResponseFile.FilePath);
        }

        public async Task Invoke(HttpContext context)
        {
            if (!_maintenanceCtrlSev.IsMaintenanceModeOn)
            {
                goto nextDelegate;
            }

            if (_options.BypassUrlPaths.Any(pathString =>
                context.Request.Path.StartsWithSegments(
                    pathString, _options.PathStringComparison)))
            {
                goto nextDelegate;
            }

            if (_options.BypassFileExtensions.Any(ext =>
                context.Request.Path.Value.EndsWith(
                    $".{ext}", _options.PathStringComparison)))
            {
                goto nextDelegate;
            }

            if (_options.BypassAuthenticatedUsers
                && context.User.Identity.IsAuthenticated)
            {
                goto nextDelegate;
            }

            if (_options.BypassUserNames.Any(userName =>
                userName == context.User.Identity.Name))
            {
                goto nextDelegate;
            }

            if (_options.BypassUserRoles.Any(role =>
                context.User.IsInRole(role)))
            {
                goto nextDelegate;
            }

            context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
            context.Response.Headers.Add("Retry-After", _options.Code503RetryAfter.ToString());
            context.Response.ContentType = _response.ContentType;
            await context
                .Response
                .WriteAsync(_response.ContentEncoding.GetString(_response.ContentBytes),
                    _response.ContentEncoding);
            return;


        nextDelegate:
            await _next.Invoke(context);
        }
    }
}