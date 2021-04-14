using MaintenanceModeMiddleware.Configuration.Data;
using Microsoft.AspNetCore.Http;
using System;

namespace MaintenanceModeMiddleware.Configuration.Options
{
    internal class BypassUrlPathOption : Option<UrlPath>, IContextMatcher
    {
        internal const char PARTS_SEPARATOR = ';';

        public override void LoadFromString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentNullException(nameof(str));
            }

            string[] parts = str.Split(PARTS_SEPARATOR);
            if (parts.Length != 2)
            {
                throw new FormatException($"{nameof(str)} is in incorrect format.");
            }

            if (!Enum.TryParse(parts[1], out StringComparison comparison))
            {
                throw new ArgumentException($"Unknown string comparison {parts[1]}");
            }

            Value = new UrlPath
            {
                Comparison = comparison,
                PathString = parts[0]
            };
        }

        public override string GetStringValue()
        {
            return $"{Value.PathString.Value}{PARTS_SEPARATOR}{Value.Comparison}";
        }

        public bool IsMatch(HttpContext context)
        {
            return context
                .Request
                .Path
                .StartsWithSegments(Value.PathString, Value.Comparison);
        }
    }
}
