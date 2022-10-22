using MaintenanceModeMiddleware.Configuration.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System;

namespace MaintenanceModeMiddleware.Configuration.Options
{
    internal class PathRedirectOption : Option<PathRedirectData>, IRedirectInitializer, IAllowedRequestMatcher
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
                Code503RetryInterval = uint.Parse(parts[1]),
                Set503ResponseCode = bool.Parse(parts[2])
            };
        }

        public override string GetStringValue()
        {
            return $"{Value.Path}{PARTS_SEPARATOR}{Value.Code503RetryInterval}{PARTS_SEPARATOR}{Value.Set503ResponseCode}";
        }

        public bool IsMatch(HttpContext context)
        {
            return context.Request.Path
                .Equals(Value.Path.ToUriComponent(), 
                    StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
