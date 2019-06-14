using Patcher.Infrastructure;
using System.Collections.Generic;

namespace Patcher.Models
{
    public class PatchInfoModel
    {
        public PatchMethod Method { get; set; }
        public List<string> List { get; set; }
    }
}
