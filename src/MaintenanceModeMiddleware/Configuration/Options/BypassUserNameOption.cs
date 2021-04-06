using System;

namespace MaintenanceModeMiddleware.Configuration.Options
{
    internal class BypassUserNameOption : Option<string>
    {
        public override void FromString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentNullException(nameof(str));
            }

            Value = str;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
