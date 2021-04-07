using System;

namespace MaintenanceModeMiddleware.Configuration.State
{
    [Serializable]
    public class StorableOption
    {
        public string TypeName { get; set; }
        public string StringValue { get; set; }
    }
}
