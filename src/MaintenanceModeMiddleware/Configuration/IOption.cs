namespace MaintenanceModeMiddleware.Configuration
{
    internal interface IOption
    {
        bool IsDefault { get; }
        string TypeName { get; }
        void LoadFromString(string str);
        string GetStringValue();
    }
}
