using Microsoft.AspNetCore.Http;
using System;

namespace MaintenanceModeMiddleware.Configuration.Options
{
    internal class BypassFileExtensionOption : Option<string>, IContextMatcher
    {
        public override void LoadFromString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentNullException(nameof(str));
            }

            Value = str;
        }

        public override string GetStringValue()
        {
            return Value;
        }

        public bool IsMatch(HttpContext context)
        {
            return context
                .Request
                .Path
                .Value
                .EndsWith(Value,
                    StringComparison.OrdinalIgnoreCase);
        }

    }
}
