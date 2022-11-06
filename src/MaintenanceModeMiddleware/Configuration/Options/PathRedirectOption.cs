using MaintenanceModeMiddleware.Configuration.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System;
using System.Threading.Tasks;

namespace MaintenanceModeMiddleware.Configuration.Options
{
    internal class PathRedirectOption : Option<PathRedirectData>,
        IRedirectInitializer, 
        IAllowedRequestMatcher,
        IRequestHandler
    {
        private const string PARTS_SEPARATOR = "[::]";
        private const string COOKIE_PREFIX = "MaintenanceReturnUrl";
        private string _cookieName;

        public override void LoadFromString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentNullException(nameof(str));
            }

            string[] parts = str.Split(PARTS_SEPARATOR);
            Value = new PathRedirectData
            {
                Path = new PathString(parts[0]),
                StatusCodeData = new ResponseStatusCodeData
                {
                    Code503RetryInterval = uint.Parse(parts[1]),
                    Set503StatusCode = bool.Parse(parts[2])
                }
            };
        }

        public override string GetStringValue()
        {
            return $"{Value.Path}{PARTS_SEPARATOR}{Value.StatusCodeData.Code503RetryInterval}{PARTS_SEPARATOR}{Value.StatusCodeData.Set503StatusCode}";
        }

        bool IAllowedRequestMatcher.IsMatch(HttpContext context)
        {
            return context.Request.Path
                .Equals(Value.Path.ToUriComponent(), 
                    StringComparison.InvariantCultureIgnoreCase);
        }

        Task IRequestHandler.Postprocess(HttpContext context)
        {
            if (context.Request.Path.Equals(Value.Path.ToUriComponent())
                &&
                Value.StatusCodeData.Set503StatusCode)
            {
                context
                   .Response
                   .StatusCode = StatusCodes.Status503ServiceUnavailable;

                context
                    .Response
                    .Headers
                    .Add("Retry-After", Value.StatusCodeData.Code503RetryInterval.ToString());
            }

            return Task.CompletedTask;
        }

        Task<PreprocessResult> IRequestHandler.Preprocess(HttpContext context)
        {
            if (Value.ReturnUrlData.SetReturnUrlInCookie)
            {
                _cookieName ??= $"{Value.ReturnUrlData.ReturnUrlCookiePrefix}_{Guid.NewGuid()}";

                if (!context.Request.Cookies.ContainsKey(_cookieName))
                {
                    context
                        .Response
                        .Cookies
                        .Append(_cookieName, context.Request.Path,
                            Value.ReturnUrlData.ReturnUrlCookieOptions ?? new CookieOptions { IsEssential = true });
                }
            }

            string returnUrlPath = Value.ReturnUrlData.CustomReturnPath.HasValue
                ? Value.ReturnUrlData.CustomReturnPath.Value.ToUriComponent()
                : Value.Path.ToUriComponent();

            string fullRedirectPath = Value.ReturnUrlData.SetReturnUrlInUrlParameter
                ? $"{Value.Path.ToUriComponent()}?{Uri.EscapeDataString(Value.ReturnUrlData.ReturnUrlUrlParameterName)}={returnUrlPath}"
                : Value.Path.ToUriComponent();

            context
                .Response
                .Redirect(fullRedirectPath);

            return Task.FromResult(new PreprocessResult { CallNext = false });
        }
    }
}
