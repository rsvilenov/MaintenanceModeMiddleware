using System.Collections.Generic;
using System.Linq;

namespace MaintenanceModeMiddleware.Configuration
{
    internal class OptionCollection
    {
        private readonly List<IOption> _options;

        public OptionCollection()
        { 
            _options = new List<IOption>();
        }

        public OptionCollection(IEnumerable<IOption> options)
        {
            _options = options.ToList();
        }

        public void Add<T>(T option)
            where T : IOption
        {
            _options.Add(option);
        }

        public T GetSingleOrDefault<T>()
            where T : IOption
        {
            return _options
                .Where(o => o is T)
                .Cast<T>()
                .SingleOrDefault();
        }

        public IEnumerable<T> GetAll<T>()
            where T : IOption
        {
            return _options
                .Where(o => o is T)
                .Cast<T>();
        }

        public IEnumerable<IOption> GetAll()
        {
            return _options.AsReadOnly();
        }

        public bool Any<T>()
            where T : IOption
        {
            return _options.Any(o => o is T);
        }

        public void Clear<T>()
            where T : IOption
        {
            _options.RemoveAll(o => o is T);
        }

        public void Clear()
        {
            _options.Clear();
        }

        public int Count 
            => _options.Count;
    }
}
