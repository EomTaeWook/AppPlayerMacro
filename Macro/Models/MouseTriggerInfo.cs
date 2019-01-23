using System;
using System.Windows;

namespace Macro.Models
{
    [Serializable]
    public class MouseTriggerInfo
    {
        public MouseEventType MouseInfoEventType { get; set; } = MouseEventType.None;
        public Point StartPoint { get; set; } = new Point();
        public Point EndPoint { get; set; } = new Point();
    }
}
