using MaintenanceModeMiddleware.Configuration.Data;
using System;
using System.Collections.Generic;

namespace MaintenanceModeMiddleware.Configuration.Options
{
    internal class ControllerActionOption : Option<ControllerActionData>, IRouteDataModifier
    {
        private const string PARTS_SEPARATOR = ":";

        public Dictionary<string, string> GetDataTokens()
        {
            return new Dictionary<string, string>
            {
                { "area", Value.AreaName }
            };
        }

        public Dictionary<string, string> GetRouteValues()
        {
            return new Dictionary<string, string>
            {
                { "controller", Value.ControllerName },
                { "action", Value.ActionName }
            };
        }

        public override string GetStringValue()
        {
            return $"{Value.AreaName}{PARTS_SEPARATOR}{Value.ControllerName}{PARTS_SEPARATOR}{Value.ActionName}";
        }

        public override void LoadFromString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentNullException(nameof(str));
            }

            string[] parts = str.Split(PARTS_SEPARATOR);
            Value = new ControllerActionData
            {
                AreaName = parts[0],
                ControllerName = parts[1],
                ActionName = parts[2]
            };
        }
    }
}
