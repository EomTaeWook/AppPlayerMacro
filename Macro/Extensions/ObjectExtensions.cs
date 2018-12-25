using Macro.Models;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Macro.Extensions
{
    public static class ObjectExtensions
    {
        public static byte[] SerializeObject(this ConfigEventModel model)
        {
            using (var ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();

                bf.Serialize(ms, '\uFF1C');
                model.Image.Save(ms, ImageFormat.Png);
                bf.Serialize(ms, '\u2194');
                bf.Serialize(ms, model.EventType);
                bf.Serialize(ms, '\u2194');
                bf.Serialize(ms, model.MousePoint);
                bf.Serialize(ms, '\u2194');
                bf.Serialize(ms, model.KeyBoardCmd);
                bf.Serialize(ms, '\u2194');
                bf.Serialize(ms, model.ProcessName);
                bf.Serialize(ms, '\uFF1E');
                return ms.ToArray();
            }
        }
    }
}
