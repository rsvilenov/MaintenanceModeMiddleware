using System;

namespace MaintenanceModeMiddleware.Configuration.Options
{
    internal class BypassFileExtensionOption : Option<string>
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
    }
}
