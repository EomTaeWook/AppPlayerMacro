using Macro.Infrastructure.Impl;

namespace Macro.Models
{
    public class SaveFileLoadModel
    {
        public BaseContentView View { get; set; }

        public string CacheFilePath { get; set; }

        public string SaveFilePath { get; set; }
    }
}
