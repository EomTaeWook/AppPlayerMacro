using Macro.Infrastructure;
using System.Drawing;
using System.Windows.Controls;
using Utils.Infrastructure;

namespace Macro.Models
{
    public interface INotifyEventArgs
    { }

    public class MousePointEventArgs : INotifyEventArgs
    {
        public MousePointViewMode MousePointViewMode { get; set; }
        public MonitorInfo MonitorInfo { get; set; }
        public MouseTriggerInfo MouseTriggerInfo { get; set; }
    }
    public class CaptureEventArgs : INotifyEventArgs
    {
        public CaptureViewMode CaptureViewMode { get; set; }
        public MonitorInfo MonitorInfo { get; set; }
        public Bitmap CaptureImage { get; set; }
        public Rect Position { get; set; }

    }
    public class ConfigEventArgs : INotifyEventArgs
    {
        public Config Config { get; set; }
    }
    public class EventTriggerOrderChangedEventArgs : INotifyEventArgs
    {
        public IBaseEventTriggerModel TriggerModel1 { get; set; }
        public IBaseEventTriggerModel TriggerModel2 { get; set; }
    }
    public class SelctTreeViewItemChangedEventArgs : INotifyEventArgs
    {
        public TreeViewItem TreeViewItem { get; set; }
    }
    public class EventTriggerEventArgs : INotifyEventArgs
    {
        public ulong Index { get; set; }
        public IBaseEventTriggerModel TriggerModel { get; set; }
    }

    public class SaveEventTriggerModelArgs : INotifyEventArgs 
    {
        public IBaseEventTriggerModel CurrentEventTriggerModel { get; set; }
    }

    public class DeleteEventTriggerModelArgs : INotifyEventArgs
    {
        public IBaseEventTriggerModel CurrentEventTriggerModel { get; set; }
    }

}
