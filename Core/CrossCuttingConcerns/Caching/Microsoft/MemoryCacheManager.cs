using Core.Utilities.IoC;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Core.CrossCuttingConcerns.Caching.Microsoft
{
    public sealed class MemoryCacheManager : ICacheManager
    {
        private readonly IMemoryCache _cache;

        // Key tracking: RemoveByPattern için güvenilir kaynak
        private static readonly ConcurrentDictionary<string, byte> _keys = new();

        public MemoryCacheManager()
        {
            _cache = ServiceTool.ServiceProvider.GetService<IMemoryCache>()
                    ?? throw new InvalidOperationException("IMemoryCache is not registered in DI container.");
        }

        public T Get<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return default!;
            return _cache.TryGetValue(key, out var value) ? (T)value! : default!;
        }

        public object Get(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return null!;
            return _cache.TryGetValue(key, out var value) ? value! : null!;
        }

        public void Add(string key, object data, int durationMinutes)
        {
            if (string.IsNullOrWhiteSpace(key)) return;

            // dakika <= 0 ise cache'e koyma (mantıklı default)
            if (durationMinutes <= 0)
            {
                Remove(key);
                return;
            }

            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(durationMinutes)
            };

            // Eviction olunca key tracking temizlensin (dictionary şişmesin)
            options.RegisterPostEvictionCallback((k, _v, _r, _s) =>
            {
                if (k is string ks) _keys.TryRemove(ks, out _);
            });

            _cache.Set(key, data, options);
            _keys[key] = 0;
        }

        public bool IsAdd(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return false;

            var exists = _cache.TryGetValue(key, out _);
            if (!exists) _keys.TryRemove(key, out _); // expire olmuşsa tracking temizliği
            return exists;
        }

        public void Remove(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return;

            _cache.Remove(key);
            _keys.TryRemove(key, out _);
        }

        public void RemoveByPattern(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern)) return;

            Regex regex;
            try
            {
                regex = new Regex(pattern,
                    RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            }
            catch
            {
                // Geçersiz regex verilirse hiçbir şey yapma (runtime kırılmasın)
                return;
            }

            // Snapshot al: enumerate sırasında değişim sorun olmasın
            var snapshot = _keys.Keys.ToArray();
            if (snapshot.Length == 0) return;

            var matched = new List<string>(capacity: Math.Min(snapshot.Length, 64));
            foreach (var key in snapshot)
            {
                if (regex.IsMatch(key))
                    matched.Add(key);
            }

            if (matched.Count == 0) return;

            foreach (var key in matched)
            {
                _cache.Remove(key);
                _keys.TryRemove(key, out _);
            }
        }
    }
}


//using Core.Utilities.IoC;
//using Microsoft.Extensions.Caching.Memory;
//using Microsoft.Extensions.DependencyInjection;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;

//namespace Core.CrossCuttingConcerns.Caching.Microsoft
//{
//    public class MemoryCacheManager : ICacheManager
//    {
//        private readonly IMemoryCache _cache;
//        public MemoryCacheManager()
//        {
//            _cache = ServiceTool.ServiceProvider.GetService<IMemoryCache>();
//        }

//        public T Get<T>(string key) => _cache.Get<T>(key);
//        public object Get(string key) => _cache.Get(key);

//        public void Add(string key, object data, int durationMinutes)
//        {
//            _cache.Set(key, data, TimeSpan.FromMinutes(durationMinutes));
//        }

//        public bool IsAdd(string key) => _cache.TryGetValue(key, out _);

//        public void Remove(string key) => _cache.Remove(key);

//        // Basit pattern temizleme: IMemoryCache entries snapshot alıp regex uygular.
//        public void RemoveByPattern(string pattern)
//        {
//            // IMemoryCache’in EntriesCollection’ına Reflection ile erişim (basit ama iş görür)
//            var type = typeof(MemoryCache);
//            var entries = type.GetProperty("EntriesCollection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
//                              ?.GetValue(_cache) as dynamic;

//            if (entries == null) return;

//            var regex = new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
//            var keysToRemove = new List<object>();

//            foreach (var item in entries)
//            {
//                ICacheEntry entry = item.GetType().GetProperty("Value").GetValue(item, null);
//                if (entry?.Key is not null && regex.IsMatch(entry.Key.ToString()))
//                    keysToRemove.Add(entry.Key);
//            }

//            foreach (var key in keysToRemove)
//                _cache.Remove(key);
//        }
//    }
//}
