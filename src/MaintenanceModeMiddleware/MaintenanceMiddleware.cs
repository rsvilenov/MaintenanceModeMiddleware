using MaintenanceModeMiddleware.Configuration;
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
        private readonly OptionCollection _options;
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
            optionsBuilder.FillEmptyOptionsWithDefault();
            _options = optionsBuilder.Options;

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

            if (_options.Any<UseDefaultResponseOption>() 
                && _options.Get<UseDefaultResponseOption>().Value)
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
            else if (_options.Any<ResponseOption>())
            {
                response = _options.Get<ResponseOption>().Value;
            }
            else if(_options.Any<ResponseFileOption>())
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
            else
            {
                throw new InvalidOperationException("Configuration error: No response was specified.");
            }

            return response;
        }

        private void VerifyOptions()
        {
            if (!_options.Any<UseDefaultResponseOption>()
                && !_options.Any<ResponseOption>()
                && !_options.Any<ResponseFileOption>())
            {
                throw new InvalidOperationException("No response was specified.");
            }

            if (_options.Any<ResponseFileOption>())
            {
                string absPath = GetAbsolutePathOfResponseFile();

                if (!File.Exists(absPath))
                {
                    throw new ArgumentException($"Could not find file {_options.Get<ResponseFileOption>().Value.FilePath}. Expected absolute path: {absPath}.");
                }
            }

            if (!_options.Any<Code503RetryIntervalOption>())
            {
                throw new ArgumentException("No value was specified for 503 retry interval.");
            }
        }

        private string GetAbsolutePathOfResponseFile()
        {
            ResponseFileOption resFileOption = _options.Get<ResponseFileOption>();
            if (resFileOption.Value.BaseDir == null)
            {
                return resFileOption.Value.FilePath;
            }

            string baseDir = resFileOption.Value.BaseDir == PathBaseDirectory.WebRootPath 
                ? _webHostEnvironment.WebRootPath 
                : _webHostEnvironment.ContentRootPath;
            
            return Path.Combine(baseDir, resFileOption.Value.FilePath);
        }

        public async Task Invoke(HttpContext context)
        {
            if (!_maintenanceCtrlSev.IsMaintenanceModeOn)
            {
                goto nextDelegate;
            }

            if (_options.GetAll<BypassUrlPathOption>().Any(o =>
                context.Request.Path.StartsWithSegments(
                    o.Value.String, o.Value.Comparison)))
            {
                goto nextDelegate;
            }

            if (_options.GetAll<BypassFileExtensionOption>().Any(o =>
                context.Request.Path.Value.EndsWith(
                    $".{o.Value}", StringComparison.OrdinalIgnoreCase)))
            {
                goto nextDelegate;
            }

            if (_options.Any<BypassAuthenticatedUsersOption>()
                && _options.Get<BypassAuthenticatedUsersOption>().Value
                && context.User.Identity.IsAuthenticated)
            {
                goto nextDelegate;
            }

            if (_options.GetAll<BypassUserNameOption>().Any(o =>
                o.Value == context.User.Identity.Name))
            {
                goto nextDelegate;
            }

            if (_options.GetAll<BypassUserRoleOption>().Any(o =>
                context.User.IsInRole(o.Value)))
            {
                goto nextDelegate;
            }

            context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
            context.Response.Headers.Add("Retry-After", _options.Get<Code503RetryIntervalOption>().Value.ToString());
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