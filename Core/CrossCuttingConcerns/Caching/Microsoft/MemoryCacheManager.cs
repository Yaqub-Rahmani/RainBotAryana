using Core.Utilities.IoC;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Core.CrossCuttingConcerns.Caching.Microsoft
{
    public class MemoryCacheManager : ICacheManager
    {
        private readonly IMemoryCache _cache;
        public MemoryCacheManager()
        {
            _cache = ServiceTool.ServiceProvider.GetService<IMemoryCache>();
        }

        public T Get<T>(string key) => _cache.Get<T>(key);
        public object Get(string key) => _cache.Get(key);

        public void Add(string key, object data, int durationMinutes)
        {
            _cache.Set(key, data, TimeSpan.FromMinutes(durationMinutes));
        }

        public bool IsAdd(string key) => _cache.TryGetValue(key, out _);

        public void Remove(string key) => _cache.Remove(key);

        // Basit pattern temizleme: IMemoryCache entries snapshot alıp regex uygular.
        public void RemoveByPattern(string pattern)
        {
            // IMemoryCache’in EntriesCollection’ına Reflection ile erişim (basit ama iş görür)
            var type = typeof(MemoryCache);
            var entries = type.GetProperty("EntriesCollection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                              ?.GetValue(_cache) as dynamic;

            if (entries == null) return;

            var regex = new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var keysToRemove = new List<object>();

            foreach (var item in entries)
            {
                ICacheEntry entry = item.GetType().GetProperty("Value").GetValue(item, null);
                if (entry?.Key is not null && regex.IsMatch(entry.Key.ToString()))
                    keysToRemove.Add(entry.Key);
            }

            foreach (var key in keysToRemove)
                _cache.Remove(key);
        }
    }
}
