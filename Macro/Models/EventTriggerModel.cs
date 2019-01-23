using Macro.Infrastructure.Serialize;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.Serialization;
using Utils.Infrastructure;

namespace Macro.Models
{
    [Serializable]
    public class EventTriggerModel : INotifyPropertyChanged
    {
        private EventType _eventType;
        private MouseTriggerInfo _mouseTriggerInfo;
        private string _keyboardCmd;
        private ProcessInfo _processInfo;
        private ObservableCollection<EventTriggerModel> _subEventTriggers;
        private int _afterDelay;

        [field:NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        [Order(1)]
        public Bitmap Image { get; set; }

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
            get => _processInfo;
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

        public string Desc
        {
            get
            {
                if (EventType == EventType.Mouse)
                {
                    if(MouseTriggerInfo.MouseInfoEventType != MouseEventType.DragAndDrop && MouseTriggerInfo.MouseInfoEventType != MouseEventType.None)
                    {
                        return $"X : { MouseTriggerInfo.StartPoint.X.ToString()} Y : {MouseTriggerInfo.StartPoint.Y.ToString() }";
                    }
                    else if(MouseTriggerInfo.MouseInfoEventType == MouseEventType.None)
                    {
                        return "";
                    }
                    else 
                    {
                        return $"X : { MouseTriggerInfo.StartPoint.X.ToString("0")} Y : {MouseTriggerInfo.StartPoint.Y.ToString("0") } / " +
                            $"X : { MouseTriggerInfo.EndPoint.X.ToString("0")} Y : {MouseTriggerInfo.EndPoint.Y.ToString("0") }";
                    }
                }
                else if (EventType == EventType.Keyboard)
                {
                    return KeyboardCmd;
                }
                else
                {
                    return "";
                }
            }
        }

        public EventTriggerModel()
        {
            _keyboardCmd = "";
            _eventType = EventType.Image;
        }
        
        public EventTriggerModel(EventTriggerModel obj)
        {
            Image = obj.Image;
            EventType = obj.EventType;
            MouseTriggerInfo = obj.MouseTriggerInfo;
            KeyboardCmd = obj.KeyboardCmd;
            ProcessInfo = obj.ProcessInfo;
            MonitorInfo = obj.MonitorInfo;
            AfterDelay = obj.AfterDelay;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
