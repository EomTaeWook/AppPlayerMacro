using System;

namespace Utils.Infrastructure
{
    [Serializable]
    public class MonitorInfo
    {
        public Rect Rect { get; set; }
        public int Index { get; set; }
    }
}
