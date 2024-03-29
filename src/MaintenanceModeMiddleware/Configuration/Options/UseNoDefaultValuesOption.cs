﻿namespace MaintenanceModeMiddleware.Configuration.Options
{
    internal class UseNoDefaultValuesOption : Option<bool>
    {
        public override void LoadFromString(string str)
        {
            Value = bool.Parse(str);
        }

        public override string GetStringValue()
        {
            return Value.ToString();
        }
    }
}
