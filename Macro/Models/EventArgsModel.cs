using System.Drawing;
using System.Windows.Controls;
using Utils.Infrastructure;

namespace Macro.Models
{
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
    public class EventTriggerEventArgs : INotifyEventArgs
    {
        public ulong Index { get; set; }
        public EventTriggerModel TriggerModel { get; set; }
    }

}
