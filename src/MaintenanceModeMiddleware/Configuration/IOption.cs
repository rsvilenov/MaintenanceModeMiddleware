namespace MaintenanceModeMiddleware.Configuration
{
    internal interface IOption
    {
        string TypeName { get; }
        void LoadFromString(string str);
        string GetStringValue();
    }
}
