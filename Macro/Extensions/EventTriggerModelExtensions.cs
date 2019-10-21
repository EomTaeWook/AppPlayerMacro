using Macro.Infrastructure;
using Macro.Models;
using System.Collections.ObjectModel;
using System.Linq;
using Utils.Infrastructure;

namespace Macro.Extensions
{
    public static class EventTriggerModelExtensions
    {
        public static EventTriggerModel Clone(this EventTriggerModel source)
        {
            return new EventTriggerModel(source);
        }
        public static void Clear(this GameEventTriggerModel source)
        {
            source.Image = null;
            source.EventType = EventType.Image;
            source.MouseTriggerInfo = new MouseTriggerInfo();
            source.MonitorInfo = new MonitorInfo();
            source.KeyboardCmd = "";
            source.ProcessInfo = new ProcessInfo();
            source.SubEventTriggers = new ObservableCollection<GameEventTriggerModel>();
            source.AfterDelay = 0;
            source.RepeatInfo = new RepeatInfoModel();
            source.EventToNext = 0;
            source.TriggerIndex = 0;
            source.HpCondition = new ValueConditionModel() { ConditionType = ConditionType.Below };
            source.MpCondition = new ValueConditionModel() { ConditionType = ConditionType.Below };
        }
        public static void Clear(this EventTriggerModel source)
        {
            source.Image = null;
            source.EventType = EventType.Image;
            source.MouseTriggerInfo = new MouseTriggerInfo();
            source.MonitorInfo = new MonitorInfo();
            source.KeyboardCmd = "";
            source.ProcessInfo = new ProcessInfo();
            source.SubEventTriggers = new ObservableCollection<EventTriggerModel>();
            source.AfterDelay = 0;
            source.RepeatInfo = new RepeatInfoModel();
            source.EventToNext = 0;
            source.TriggerIndex = 0;
        }
        public static ValueConditionModel Clone(this ValueConditionModel source)
        {
            return new ValueConditionModel()
            {
                ConditionType = source.ConditionType,
                Value = source.Value
            };
        }
        public static MouseTriggerInfo Clone(this MouseTriggerInfo source)
        {
            return new MouseTriggerInfo()
            {
                EndPoint = new System.Windows.Point()
                {
                    X = source.EndPoint.X,
                    Y = source.EndPoint.Y
                },
                MiddlePoint = source.MiddlePoint.Select(r=>  new System.Windows.Point()
                {
                    X = r.X,
                    Y = r.Y
                }).ToList(),
                MouseInfoEventType = source.MouseInfoEventType,
                StartPoint = new System.Windows.Point()
                {
                    X = source.StartPoint.X,
                    Y = source.StartPoint.Y
                },
                WheelData = source.WheelData
            };
        }
        public static Rect Clone(this Rect source)
        {
            return new Rect()
            {
                Bottom = source.Bottom,
                Left = source.Left,
                Right = source.Right,
                Top = source.Top
            };
        }
        public static ProcessInfo Clone(this ProcessInfo source)
        {
            return new ProcessInfo()
            {
                Position = source.Position.Clone(),
                ProcessName = source.ProcessName.Clone() as string
            };
        }
        public static RepeatInfoModel Clone(this RepeatInfoModel source)
        {
            return new RepeatInfoModel()
            {
                Count = source.Count,
                RepeatType = source.RepeatType,
            };
        }
        public static MonitorInfo Clone(this MonitorInfo source)
        {
            return new MonitorInfo()
            {
                Dpi = new System.Drawing.Point()
                {
                    X = source.Dpi.X,
                    Y = source.Dpi.Y
                },
                Index = source.Index,
                Rect = source.Rect.Clone()
            };
        }
        public static ulong CompareIndex(this EventTriggerModel source, ulong index)
        {
            if (source.TriggerIndex > index)
            {
                index = source.TriggerIndex;
            }
            foreach (var child in source.SubEventTriggers)
            {
                index = child.CompareIndex(index);
            }
            return index;
        }
    }
}
