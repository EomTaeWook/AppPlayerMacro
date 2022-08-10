using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patcher.Models
{
    public class PatchListModel
    {
        public PatchInfoModel CurrentVersion { get; set; }

        public List<PatchInfoModel> OldVersion { get; set; }
    }
}
