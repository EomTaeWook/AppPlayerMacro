using System.Drawing;
using Utils.Infrastructure;
using Point = System.Windows.Point;

namespace Macro.Models.Event
{
    public class MousePointArgs
    {
        public MonitorInfo MonitorInfo { get; set; }
        public Point? MousePoint { get; set; }
    }
    public class CaptureArgs
    {
        public MonitorInfo MonitorInfo { get; set; }
        public Bitmap CaptureImage { get; set; }

    }

}
