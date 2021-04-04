using MaintenanceModeMiddleware.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MaintenanceModeMiddleware
{
    public class MaintenanceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMaintenanceControlService _maintenanceCtrlSev;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly MiddlewareOptionsBuilder _options;
        private readonly MaintenanceResponse _response;

        public MaintenanceMiddleware(RequestDelegate next,
            IMaintenanceControlService maintenanceCtrlSev,
            IWebHostEnvironment webHostEnvironment,
            Action<MiddlewareOptionsBuilder> options)
        {
            _next = next;
            _maintenanceCtrlSev = maintenanceCtrlSev;
            _webHostEnvironment = webHostEnvironment;

            _options = new MiddlewareOptionsBuilder();
            options?.Invoke(_options);
            _options.FillEmptyOptionsWithDefault();

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
            MaintenanceResponse response;

            if (_options.UseDefaultResponse)
            {
                Stream resStream = GetType()
                    .Assembly
                    .GetManifestResourceStream($"{nameof(MaintenanceModeMiddleware)}.Resources.DefaultResponse.html");
                if (resStream == null)
                {
                    throw new InvalidOperationException("The default response resource could not be found.");
                }

                using var resSr = new StreamReader(resStream, Encoding.UTF8);
                response = new MaintenanceResponse
                {
                    ContentBytes = resSr.CurrentEncoding.GetBytes(resSr.ReadToEnd()),
                    ContentEncoding = resSr.CurrentEncoding,
                    ContentType = ContentType.Html
                };
            }
            else if (_options.Response != null)
            {
                response = _options.Response;
            }
            else
            {
                string absPath = GetAbsolutePathOfResponseFile();
                using StreamReader sr = new StreamReader(absPath, detectEncodingFromByteOrderMarks: true);

                response = new MaintenanceResponse
                {
                    ContentBytes = sr.CurrentEncoding.GetBytes(sr.ReadToEnd()),
                    ContentEncoding = sr.CurrentEncoding,
                    ContentType = absPath.EndsWith(".txt")
                        ? ContentType.Text : ContentType.Html
                };
            }

            return response;
        }

        private void VerifyOptions()
        {
            if (!_options.UseDefaultResponse
                && _options.Response == null 
                && _options.ResponseFile != null)
            {
                string absPath = GetAbsolutePathOfResponseFile();

                if (!System.IO.File.Exists(absPath))
                {
                    throw new ArgumentException($"Could not find file {_options.ResponseFile.FilePath}, specified for option {nameof(_options.ResponseFile)}. Expected absolute path: {absPath}.");
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

            if (_options.UrlPathsToBypass.Any(urlPath =>
                context.Request.Path.StartsWithSegments(
                    urlPath.String, urlPath.Comparison)))
            {
                goto nextDelegate;
            }

            if (_options.FileExtensionsToBypass.Any(ext =>
                context.Request.Path.Value.EndsWith(
                    $".{ext}", StringComparison.OrdinalIgnoreCase)))
            {
                goto nextDelegate;
            }

            if (_options.BypassAuthenticatedUsers
                && context.User.Identity.IsAuthenticated)
            {
                goto nextDelegate;
            }

            if (_options.UserNamesToBypass.Any(userName =>
                userName == context.User.Identity.Name))
            {
                goto nextDelegate;
            }

            if (_options.UserRolesToBypass.Any(role =>
                context.User.IsInRole(role)))
            {
                goto nextDelegate;
            }

            context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
            context.Response.Headers.Add("Retry-After", _options.Code503RetryAfter.ToString());
            context.Response.ContentType = _response.GetContentTypeString();

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