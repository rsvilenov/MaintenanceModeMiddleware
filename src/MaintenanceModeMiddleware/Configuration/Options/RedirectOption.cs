using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

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
            Value = str;
        }

        public bool IsMatch(HttpContext context)
        {
            string fullUrl = UriHelper.GetDisplayUrl(context.Request);

            return fullUrl
                .Equals(Value, StringComparison.InvariantCultureIgnoreCase);
        }

    }
}
