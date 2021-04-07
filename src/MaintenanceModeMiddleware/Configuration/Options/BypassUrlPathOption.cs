using MaintenanceModeMiddleware.Configuration.Data;
using System;

namespace MaintenanceModeMiddleware.Configuration.Options
{
    internal class BypassUrlPathOption : Option<UrlPath>
    {
        private const char PARTS_SEPARATOR = ';';

        public override void FromString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentNullException(nameof(str));
            }

            string[] parts = str.Split(PARTS_SEPARATOR);
            if (parts.Length != 2)
            {
                throw new ArgumentException($"{nameof(str)} is in incorrect format.");
            }

            if (!Enum.TryParse(parts[1], out StringComparison comparison))
            {
                throw new ArgumentException($"Unknown string comparison {parts[1]}");
            }

            Value = new UrlPath
            {
                Comparison = comparison,
                String = parts[0]
            };
        }

        public override string ToString()
        {
            return $"{Value.String.Value}{PARTS_SEPARATOR}{Value.Comparison}";
        }
    }
}
