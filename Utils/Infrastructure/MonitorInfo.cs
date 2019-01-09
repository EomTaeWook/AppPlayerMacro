using System;
using System.Drawing;

namespace Utils.Infrastructure
{
    [Serializable]
    public class MonitorInfo
    {
        public Rect Rect { get; set; }
        public int Index { get; set; }
        public Point Dpi { get; set; } = new Point();
    }
}
