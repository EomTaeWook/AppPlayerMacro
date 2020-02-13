using Macro.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Drawing;

namespace Macro.Models
{
    [Serializable]
    public class EventTriggerModel : BaseEventTriggerModel<EventTriggerModel>
    {
        public static EventTriggerModel DummyParentEventModel;
        public EventTriggerModel()
        {
        }

        public EventTriggerModel(EventTriggerModel model)
        {
            Image = model.Image.Clone() as Bitmap;
            EventType = model.EventType;
            MouseTriggerInfo = model.MouseTriggerInfo.Clone();
            MonitorInfo = model.MonitorInfo.Clone();
            KeyboardCmd = model.KeyboardCmd.Clone() as string;
            ProcessInfo = model.ProcessInfo.Clone();
            _subEventTriggers = new ObservableCollection<EventTriggerModel>();
            foreach(var item in model.SubEventTriggers)
            {
                _subEventTriggers.Add(item);
            }
            AfterDelay = model.AfterDelay;
            RepeatInfo = model.RepeatInfo.Clone();
            EventToNext = model.EventToNext;
            _triggerIndex = 0;
            SameImageDrag = model.SameImageDrag;
        }
    }
}
