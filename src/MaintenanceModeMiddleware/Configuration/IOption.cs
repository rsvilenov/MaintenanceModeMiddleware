namespace MaintenanceModeMiddleware.Configuration
{
    internal interface IOption
    {
        bool IsDefault { get; }
        string TypeName { get; }
        void FromString(string str);
        string ToString();
    }
}
