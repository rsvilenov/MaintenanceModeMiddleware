using Microsoft.AspNetCore.Hosting;

namespace MaintenanceModeMiddleware.Configuration
{
    internal abstract class Option<T> : ISerializableOption
    {
        public T Value { get; set; }
        public virtual void Verify(IWebHostEnvironment webHostEnv)
        { }

        public string TypeName => GetType().Name;
        public abstract string GetStringValue();
        public abstract void LoadFromString(string str);
    }
}
