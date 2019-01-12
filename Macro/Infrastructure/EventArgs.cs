using Macro.Models;
using System.Drawing;
using Utils.Infrastructure;
using Point = System.Windows.Point;

namespace Macro.Infrastructure
{
    public enum EventType
    {
        ConfigChanged,
        MousePointDataBind,
        ScreenCapture,

        Max
    }
    public interface INotifyEventArgs
    { }

    public class MousePointEventArgs : INotifyEventArgs
    {
        public MonitorInfo MonitorInfo { get; set; }
        public Point? MousePoint { get; set; }
    }
    public class CaptureEventArgs : INotifyEventArgs
    {
        public MonitorInfo MonitorInfo { get; set; }
        public Bitmap CaptureImage { get; set; }

    }
    public class ConfigEventArgs : INotifyEventArgs
    {
        public Config Config { get; set; }
    }
}
