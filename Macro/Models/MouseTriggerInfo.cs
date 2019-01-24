using System;
using System.Collections.Generic;
using System.Windows;

namespace Macro.Models
{
    [Serializable]
    public class MouseTriggerInfo
    {
        public MouseEventType MouseInfoEventType { get; set; } = MouseEventType.None;
        public Point StartPoint { get; set; } = new Point();
        public List<Point> MiddlePoint { get; set; } = new List<Point>();
        public Point EndPoint { get; set; } = new Point();
    }
}
