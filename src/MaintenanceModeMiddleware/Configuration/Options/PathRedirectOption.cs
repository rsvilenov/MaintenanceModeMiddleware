using MaintenanceModeMiddleware.Configuration.Data;
using Microsoft.AspNetCore.Http;
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

        public string RedirectLocation => Value.Path.ToUriComponent();

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
            context
                .Response
                .Redirect(RedirectLocation);

            return Task.FromResult(new PreprocessResult { CallNext = false });
        }
    }
}
