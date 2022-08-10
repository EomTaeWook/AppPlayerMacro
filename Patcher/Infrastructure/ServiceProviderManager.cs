using KosherUtils.Framework;
using System;
using System.Collections.Generic;
using Utils.Document;

namespace Patcher.Infrastructure
{
    public class ServiceProviderManager : Singleton<ServiceProvider>
    {
        private static Dictionary<string, object> _typeToMap = new Dictionary<string, object>();

        private static Dictionary<string, int> _enumToMap = new Dictionary<string, int>();
        public static void AddService<T>(string key, T obj) where T : class
        {
            _typeToMap.Add(key, obj);
        }
        public static void AddRefService<T>(string key, T obj) where T : struct
        {
            _typeToMap.Add(key, obj);
        }
        public static T GetService<T>(string key) where T : class
        {
            if(_typeToMap.ContainsKey(key) == false)
            {
                throw new KeyNotFoundException();
            }
            return _typeToMap[key] as T;
        }
        public static T GetRefService<T>(string key) where T : struct
        {
            if (_typeToMap.ContainsKey(key) == false)
            {
                throw new KeyNotFoundException();
            }
            return (T)_typeToMap[key];
        }
    }
}
