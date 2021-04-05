namespace MaintenanceModeMiddleware.Configuration
{
    internal class Option<T> : IOption
    {
        public Option(T value, bool isDefault = false)
        {
            Value = value;
            IsDefault = isDefault;
        }

        public T Value { get; }
        public bool IsDefault { get; }
    }

    internal interface IOption
    {
        bool IsDefault { get; }
    }
}
