using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public class JsonHelper
    {
        public static T Load<T>(string path)
        {
            var json = File.ReadAllText(path);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }
    }
}
