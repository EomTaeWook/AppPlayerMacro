using System;

namespace Utils
{
    public class Singleton<T>
    {
        private static readonly Lazy<T> _instance = new Lazy<T>();
        protected Singleton()
        {
        }
        public static T Instance
        {
           get => _instance.Value;
        }
    }
}
