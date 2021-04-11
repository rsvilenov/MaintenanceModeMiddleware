using MaintenanceModeMiddleware.Configuration;
using MaintenanceModeMiddleware.Configuration.Builders;
using MaintenanceModeMiddleware.Configuration.Data;
using MaintenanceModeMiddleware.Configuration.Enums;
using MaintenanceModeMiddleware.Configuration.Options;
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
        private readonly OptionCollection _startupOptions;
        private readonly MaintenanceResponse _response;

        public MaintenanceMiddleware(RequestDelegate next,
            IMaintenanceControlService maintenanceCtrlSev,
            IWebHostEnvironment webHostEnvironment,
            Action<MiddlewareOptionsBuilder> options)
        {
            _next = next;
            _maintenanceCtrlSev = maintenanceCtrlSev;
            _webHostEnvironment = webHostEnvironment;

            MiddlewareOptionsBuilder optionsBuilder = new MiddlewareOptionsBuilder();
            options?.Invoke(optionsBuilder);
            _startupOptions = optionsBuilder.GetOptions();

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

            if (_startupOptions
                .GetSingleOrDefault<UseDefaultResponseOption>()
                ?.Value == true)
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
            else if (_startupOptions.Any<ResponseOption>())
            {
                response = _startupOptions.GetSingleOrDefault<ResponseOption>().Value;
            }
            else if(_startupOptions.Any<ResponseFileOption>())
            {
                string absPath = GetAbsolutePathOfResponseFile();
                using StreamReader sr = new StreamReader(absPath, detectEncodingFromByteOrderMarks: true);

                ContentType contentType = Path.GetExtension(absPath) switch
                {
                    ".txt" => ContentType.Text,
                    ".html" => ContentType.Html,
                    ".json" => ContentType.Json,
                    _ => throw new InvalidOperationException($"Path {absPath} is not in any of the supported formats."),
                };

                response = new MaintenanceResponse
                {
                    ContentBytes = sr.CurrentEncoding.GetBytes(sr.ReadToEnd()),
                    ContentEncoding = sr.CurrentEncoding,
                    ContentType = contentType
                };
            }
            else
            {
                throw new InvalidOperationException("Configuration error: No response was specified.");
            }

            return response;
        }

        private void VerifyOptions()
        {
            if (!_startupOptions.Any<UseDefaultResponseOption>()
                && !_startupOptions.Any<ResponseOption>()
                && !_startupOptions.Any<ResponseFileOption>())
            {
                throw new ArgumentException("No response was specified.");
            }

            if (_startupOptions.Any<ResponseFileOption>())
            {
                string absPath = GetAbsolutePathOfResponseFile();

                if (!File.Exists(absPath))
                {
                    throw new ArgumentException($"Could not find file {_startupOptions.GetSingleOrDefault<ResponseFileOption>().Value.FilePath}. Expected absolute path: {absPath}.");
                }
            }

            if (!_startupOptions.Any<Code503RetryIntervalOption>())
            {
                throw new ArgumentException("No value was specified for 503 retry interval.");
            }
        }

        private string GetAbsolutePathOfResponseFile()
        {
            ResponseFileOption resFileOption = _startupOptions.GetSingleOrDefault<ResponseFileOption>();
            
            string baseDir = resFileOption.Value.BaseDir == PathBaseDirectory.WebRootPath 
                ? _webHostEnvironment.WebRootPath 
                : _webHostEnvironment.ContentRootPath;
            
            return Path.Combine(baseDir, resFileOption.Value.FilePath);
        }

        private OptionCollection GetOptionCollection()
        {
            ICanOverrideMiddlewareOptions optionsOverrider =
                _maintenanceCtrlSev as ICanOverrideMiddlewareOptions;

            return optionsOverrider?.GetOptionsToOverride()
                ?? _startupOptions;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!_maintenanceCtrlSev.IsMaintenanceModeOn)
            {
                goto nextDelegate;
            }

            OptionCollection options = GetOptionCollection();
            if (options == null)
            {
                goto nextDelegate;
            }

            if (options.GetAll<BypassUrlPathOption>().Any(o =>
                context.Request.Path.StartsWithSegments(
                    o.Value.PathString, o.Value.Comparison)))
            {
                goto nextDelegate;
            }

            if (options.GetAll<BypassFileExtensionOption>().Any(o =>
                context.Request.Path.Value.EndsWith(
                    $".{o.Value}", StringComparison.OrdinalIgnoreCase)))
            {
                goto nextDelegate;
            }

            if (options.GetSingleOrDefault<BypassAllAuthenticatedUsersOption>()?.Value == true
                && context.User.Identity.IsAuthenticated)
            {
                goto nextDelegate;
            }

            if (options.GetAll<BypassUserNameOption>().Any(o =>
                o.Value == context.User.Identity.Name))
            {
                goto nextDelegate;
            }

            if (options.GetAll<BypassUserRoleOption>().Any(o =>
                context.User.IsInRole(o.Value)))
            {
                goto nextDelegate;
            }

            context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
            context.Response.Headers.Add("Retry-After", options.GetSingleOrDefault<Code503RetryIntervalOption>().Value.ToString());
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