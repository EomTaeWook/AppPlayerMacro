using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
