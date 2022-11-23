using System.Collections.Generic;
using MaintenanceModeMiddleware.Configuration.Data;

namespace MaintenanceModeMiddleware.Configuration
{
    internal interface IRouteDataModifier : IOption
    {
        Dictionary<string, string> GetRouteValues();
        Dictionary<string, string> GetDataTokens();
    }
}
