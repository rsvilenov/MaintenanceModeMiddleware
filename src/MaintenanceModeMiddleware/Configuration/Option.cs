namespace MaintenanceModeMiddleware.Configuration
{
    internal abstract class Option<T> : IOption
    {
        public T Value { get; set; }

        // the members below are used for serialization
        public string TypeName => GetType().Name;
        public abstract string GetStringValue();
        public abstract void LoadFromString(string str);
    }
}
