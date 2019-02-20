using Macro.Models;
using System.Drawing;
using System.Windows.Controls;
using Utils.Infrastructure;

namespace Macro.Infrastructure
{
    public enum EventType
    {
        ConfigChanged,
        MousePointDataBind,
        ScreenCapture,
        TreeItemOrderChanged,
        SelctTreeViewItemChanged,
        EventTriggerOrderChanged,

        Max
    }
    public interface INotifyEventArgs
    { }

    public class MousePointEventArgs : INotifyEventArgs
    {
        public MonitorInfo MonitorInfo { get; set; }
        public MouseTriggerInfo MouseTriggerInfo { get; set; }
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
    public class EventTriggerOrderChangedEventArgs : INotifyEventArgs
    {
        public EventTriggerModel TriggerModel1 { get; set; }
        public EventTriggerModel TriggerModel2 { get; set; }
    }
    public class SelctTreeViewItemChangedEventArgs : INotifyEventArgs
    {
        public TreeViewItem TreeViewItem { get; set; }
    }
}
