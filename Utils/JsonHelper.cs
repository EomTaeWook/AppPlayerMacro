using System.IO;

namespace Utils
{
    public class JsonHelper
    {
        public static T Load<T>(string path)
        {
            var json = File.ReadAllText(path);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }
        public static string SerializeObject(object obj)
        {
#if DEBUG
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
#else
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.None);
#endif

        }
    }
}
