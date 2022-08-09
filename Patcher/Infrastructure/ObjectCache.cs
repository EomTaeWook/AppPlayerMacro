using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Patcher.Infrastructure
{
    public class ObjectCache
    {
        private static ConcurrentDictionary<string, object> _caches = new ConcurrentDictionary<string, object>();
        public static void SetValue(string key, object value)
        {
            _caches.AddOrUpdate(key, value, (newKey, newValue) => 
            {
                value = newValue;
                return value;
            });
        }
        public static object GetValue(string key)
        {
            if (!_caches.TryGetValue(key, out object obj))
            {
                return null;
            }
            return obj;
        }
        public static KeyValuePair<string, object>[] GetCaches()
        {
            return _caches.ToArray();
        }
    }
}
