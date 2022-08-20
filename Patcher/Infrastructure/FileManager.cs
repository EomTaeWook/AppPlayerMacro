using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Patcher.Infrastructure
{
    public class FileManager
    {
        public void Move(string targetPath, DirectoryInfo sourceDirectory, bool isOverrideMode = false)
        {
            foreach (var item in sourceDirectory.GetDirectories())
            {
                Move(targetPath, item);
            }

            var directory = Directory.CreateDirectory(targetPath);
            directory.Create();

            var buffer = new byte[4096];
            var removed = new List<FileInfo>();
            foreach (var item in sourceDirectory.GetFiles())
            {
                if (item.Name == AppDomain.CurrentDomain.FriendlyName)
                {
                    continue;
                }

                if (ConstHelper.ExcludeExtension.Any(r => r.Equals(item.Extension)))
                {
                    continue;
                }

                var read = 0;

                var newFileInfo = new FileInfo($"{targetPath}{item.Name}");

                if(newFileInfo.Exists == true && isOverrideMode == false)
                {
                    continue;
                }

                using (var rs = item.OpenRead())
                {
                    using (var ws = newFileInfo.OpenWrite())
                    {
                        while ((read = rs.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            ws.Write(buffer, 0, read);
                        }
                        ws.Flush();
                        ws.Close();
                    }
                    rs.Close();
                }
                removed.Add(item);
            }

            foreach (var item in removed)
            {
                item.Delete();
            }
            sourceDirectory.Delete(true);
        }
    }
}
