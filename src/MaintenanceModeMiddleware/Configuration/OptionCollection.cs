﻿using System.Collections.Generic;
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
             => _options.Add(option);

        public T Get<T>()
            where T : IOption
            => _options
                .Where(o => o is T)
                .Cast<T>()
                .FirstOrDefault();

        public IEnumerable<T> GetAll<T>()
            where T : IOption
            => _options
                .Where(o => o is T)
                .Cast<T>();

        public IEnumerable<IOption> GetAll()
            => _options;

        public bool Any<T>()
            where T : IOption
            => _options
                .Any(o => o is T);

        public void Clear<T>() =>
            _options.RemoveAll(o => o is T);

        public void Clear() =>
            _options.Clear();

        public int Count => _options.Count;
    }
}