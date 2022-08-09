using KosherUtils.Framework;
using KosherUtils.Log;
using Macro.Infrastructure.Serialize;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Utils;

namespace Macro.Infrastructure.Manager
{
    public class FileManager : Singleton<FileManager>
    {
        public FileManager()
        {
            
        }

        public bool SaveJson<T>(string path, T model)
        {
            try
            {
                var json = JsonHelper.SerializeObject(model, true);
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);

                return false;
            }

            return true;
        }

        public async Task<List<T>> Load<T>(string path)
        {
            try
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
                {
                    byte[] buffer = new byte[stream.Length];
                    var datas = new List<byte>();
                    var read = 0;
                    while((read = await stream.ReadAsync(buffer, 0, (int)stream.Length)) != 0)
                    {
                        datas.AddRange(buffer.Take(read));
                    }
                    return ObjectSerializer.DeserializeObject<T>(datas.ToArray());
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
            return null;
        }

        public async Task Save<T>(string path, ObservableCollection<T> list)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using (var fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                foreach (var item in list)
                {
                    var bytes = ObjectSerializer.SerializeObject(item);
                    await fs.WriteAsync(bytes, 0, bytes.Count());
                }
                fs.Close();
            }
        }
        public async void Save<T>(string path, List<T> list)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using (var fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                foreach (var item in list)
                {
                    var bytes = ObjectSerializer.SerializeObject(item);
                    await fs.WriteAsync(bytes, 0, bytes.Count());
                }
                fs.Close();
            }
        }
    }
}
