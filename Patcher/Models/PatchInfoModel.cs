using System.Collections.Generic;
using Utils.Infrastructure;

namespace Patcher.Models
{
    public class PatchInfoModel
    {
        public string Version { get; set; }
        public List<string> List { get; set; }

        private Dictionary<string, string> fileToMap;

        public Dictionary<string, string> GetFileList()
        {
            if (fileToMap != null)
            {
                return fileToMap;
            }
            fileToMap = new Dictionary<string, string>();
            foreach(var item in List)
            {
                var splits = item.Split(' ');
                fileToMap.Add(splits[0], splits[1]);
            }
            return fileToMap;
        }
        public Version GetVersion()
        {
            return Utils.Infrastructure.Version.MakeVersion(this.Version);
        }
    }
}
