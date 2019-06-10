using Macro.Infrastructure.Serialize;
using System;

namespace Macro.Models
{
    [Serializable]
    public class CacheModel
    {
        public CacheModel() : this(0)
        {
        }
        public CacheModel(ulong maxIndex)
        {
            LatestCheckDateTime = DateTime.Now.Ticks;
            MaxIndex = maxIndex;
        }
        [Order(1)]
        public ulong MaxIndex { get; set; }
        [Order(2)]
        public long LatestCheckDateTime { get; set; }
    }
}
