using Microsoft.AspNetCore.Http;
using System;

namespace MaintenanceModeMiddleware.Configuration.Options
{
    internal class UseRedirectOption : Option<PathString>, IRedirectInitializer
    {
        public PathString RedirectPath => Value;

        public override void LoadFromString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentNullException(nameof(str));
            }

            Value = new PathString(str);
        }

        public override string GetStringValue()
        {
            return Value.ToString();
        }
    }
}
