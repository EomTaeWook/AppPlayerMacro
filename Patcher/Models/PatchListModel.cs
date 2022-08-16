using System.Collections.Generic;

namespace Patcher.Models
{
    public class PatchListModel
    {
        public PatchInfoModel CurrentVersion { get; set; }

        public List<PatchInfoModel> OldVersion { get; set; }
    }
}
