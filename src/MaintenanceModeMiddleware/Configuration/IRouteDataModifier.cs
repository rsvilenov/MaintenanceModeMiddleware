using System.Collections.Generic;

namespace MaintenanceModeMiddleware.Configuration
{
    internal interface IRouteDataModifier : IOption
    {
        Dictionary<string, string> GetRouteValues();
        Dictionary<string, string> GetDataTokens();
    }
}
