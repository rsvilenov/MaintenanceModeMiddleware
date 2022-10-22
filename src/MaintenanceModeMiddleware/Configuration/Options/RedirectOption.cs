using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System;

namespace MaintenanceModeMiddleware.Configuration.Options
{
    internal class RedirectOption : Option<string>, IRedirectInitializer, IAllowedRequestMatcher
    {
        public string RedirectLocation => Value;

        public override string GetStringValue()
        {
            return Value;
        }

        public override void LoadFromString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentNullException(nameof(str));
            }

            if (!Uri.IsWellFormedUriString(str, UriKind.RelativeOrAbsolute))
            {
                throw new ArgumentException("The restored url is not well formatted.", paramName: nameof(str));
            }

            Value = str;
        }

        public bool IsMatch(HttpContext context)
        {
            return UriHelper
                .GetDisplayUrl(context.Request)
                .Equals(Value, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
