using Macro.Infrastructure;
using Macro.Infrastructure.Serialize;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using Utils.Infrastructure;

namespace Macro.Models
{
    [Serializable]
    public class EventTriggerModel : INotifyPropertyChanged
    {
        public static EventTriggerModel DummyParentEventModel;

        private EventType _eventType = EventType.Image;
        private MouseTriggerInfo _mouseTriggerInfo;
        private string _keyboardCmd = "";
        private ProcessInfo _processInfo;
        private ObservableCollection<EventTriggerModel> _subEventTriggers;
        private int _afterDelay;
        private RepeatInfoModel _repeatInfo;
        private ulong _eventToNext = 0;
        private ulong _triggerIndex = 0;
        private bool _imageSearchRequired = false;
        private bool _sameImageDrag = false;
        private int _maxSameImageCount = 1;
        private Bitmap _image;

        public EventTriggerModel()
        {
        }
        public EventTriggerModel(EventTriggerModel other)
        {
            _image = other.Image;
            _eventType = other.EventType;
            _mouseTriggerInfo = other.MouseTriggerInfo;
            _keyboardCmd = other.KeyboardCmd;
            _processInfo = other.ProcessInfo;
            _subEventTriggers = other.SubEventTriggers;
            _afterDelay = other.AfterDelay;
            _repeatInfo = other.RepeatInfo;
            _eventToNext = other.EventToNext;
            _triggerIndex = other.TriggerIndex;
            _imageSearchRequired = other.ImageSearchRequired;
            _sameImageDrag = other.SameImageDrag;
            _maxSameImageCount = other.MaxSameImageCount;
        }

        [Order(1)]
        public Bitmap Image 
        { 
            get => _image;
            set => _image = value; 
        }

        [Order(2)]
        public EventType EventType
        {
            get => _eventType;
            set
            {
                _eventType = value;
                OnPropertyChanged("EventType");
                OnPropertyChanged("Desc");
            }
        }

        [Order(3)]
        public MouseTriggerInfo MouseTriggerInfo
        {
            get => _mouseTriggerInfo ?? (_mouseTriggerInfo = new MouseTriggerInfo());
            set
            {
                _mouseTriggerInfo = value;
                OnPropertyChanged("MouseTriggerInfo");
                OnPropertyChanged("Desc");
            }
        }

        [Order(4)]
        public MonitorInfo MonitorInfo { get; set; }

        [Order(5)]
        public string KeyboardCmd
        {
            get => _keyboardCmd;
            set
            {
                _keyboardCmd = value;
                OnPropertyChanged("KeyboardCmd");
                OnPropertyChanged("Desc");
            }
        }

        [Order(6)]
        public ProcessInfo ProcessInfo
        {
            get => _processInfo ?? (_processInfo = new ProcessInfo() { Position = new Rect(), });
            set
            {
                _processInfo = value;
                OnPropertyChanged("ProcessInfo");
            }
        }

        [Order(7)]
        public ObservableCollection<EventTriggerModel> SubEventTriggers
        {
            get => _subEventTriggers ?? (_subEventTriggers = new ObservableCollection<EventTriggerModel>());
            set
            {
                _subEventTriggers = value;
                OnPropertyChanged("SubEventTriggers");
            }
        }

        [Order(8)]
        public int AfterDelay
        {
            get => _afterDelay;
            set
            {
                _afterDelay = value;
                OnPropertyChanged("AfterDelay");
            }
        }
        [Order(9)]
        public RepeatInfoModel RepeatInfo
        {
            get => _repeatInfo ?? (_repeatInfo = new RepeatInfoModel());
            set
            {
                _repeatInfo = value;
                OnPropertyChanged("RepeatInfo");
            }
        }
        [Order(10)]
        public ulong TriggerIndex
        {
            set
            {
                _triggerIndex = value;
                OnPropertyChanged("TriggerIndex");
            }
            get => _triggerIndex;
        }

        [Order(11)]
        public ulong EventToNext
        {
            set
            {
                _eventToNext = value;
                OnPropertyChanged("EventToNext");
            }
            get => _eventToNext;
        }
        [Order(12)]
        public bool ImageSearchRequired
        {
            set
            {
                _imageSearchRequired = value;
                OnPropertyChanged("ImageSearchRequired");
            }
            get => _imageSearchRequired;
        }
        [Order(13)]
        public bool SameImageDrag
        {
            set
            {
                _sameImageDrag = value;
                OnPropertyChanged("SameImageDrag");
            }
            get => _sameImageDrag;
        }
        [Order(14)]
        public int MaxSameImageCount
        {
            set
            {
                _maxSameImageCount = value;
                OnPropertyChanged("MaxSameImageCount");
            }
            get => _maxSameImageCount;
        }
        public string Desc
        {
            get
            {
                if (EventType == EventType.Mouse)
                {
                    if (MouseTriggerInfo.MouseInfoEventType != MouseEventType.Drag && MouseTriggerInfo.MouseInfoEventType != MouseEventType.None && MouseTriggerInfo.MouseInfoEventType != MouseEventType.Wheel)
                    {
                        return $"X : { MouseTriggerInfo.StartPoint.X} Y : {MouseTriggerInfo.StartPoint.Y}";
                    }
                    else if (MouseTriggerInfo.MouseInfoEventType == MouseEventType.None)
                    {
                        return "";
                    }
                    else if (MouseTriggerInfo.MouseInfoEventType == MouseEventType.Wheel)
                    {
                        if (MouseTriggerInfo.WheelData > 0)
                        {
                            return $"Wheel Up";
                        }
                        else
                        {
                            return $"Wheel Down";
                        }
                    }
                    else
                    {
                        return $"X : { MouseTriggerInfo.StartPoint.X:0} Y : {MouseTriggerInfo.StartPoint.Y:0}\r\n" +
                            $"X : { MouseTriggerInfo.EndPoint.X:0} Y : {MouseTriggerInfo.EndPoint.Y:0}";
                    }
                }
                else if (EventType == EventType.Keyboard)
                {
                    return KeyboardCmd;
                }
                else if (EventType == EventType.RelativeToImage)
                {
                    return $"X : { MouseTriggerInfo.StartPoint.X} Y : {MouseTriggerInfo.StartPoint.Y}";
                }
                else
                {
                    return "";
                }
            }
        }
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
