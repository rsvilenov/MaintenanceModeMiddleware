namespace MaintenanceModeMiddleware.Configuration
{
    internal interface ISerializableOption : IOption
    {
        string TypeName { get; }
        void LoadFromString(string str);
        string GetStringValue();
    }
}
