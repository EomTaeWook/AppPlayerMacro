using Macro.Infrastructure.Serialize;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Utils.Infrastructure;
using Point = System.Windows.Point;

namespace Macro.Models
{
    [Serializable]
    public class EventTriggerModel : INotifyPropertyChanged
    {
        private EventType _eventType;
        private Point? _mousePoint;
        private string _keyboardCmd;
        private ProcessInfo _processInfo;
        private int _afterDelay;

        public event PropertyChangedEventHandler PropertyChanged;

        [Order(1)]
        public int Index { get; set; }

        [Order(2)]
        public Bitmap Image { get; set; }

        [Order(3)]
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

        [Order(4)]
        public Point? MousePoint
        {
            get => _eventType == EventType.Mouse ? _mousePoint : null;
            set
            {
                _mousePoint = value;
                OnPropertyChanged("MousePoint");
                OnPropertyChanged("Desc");
            }
        }

        [Order(5)]
        public MonitorInfo MonitorInfo { get; set; }

        [Order(6)]
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

        [Order(7)]
        public ProcessInfo ProcessInfo
        {
            get => _processInfo;
            set
            {
                _processInfo = value;
                OnPropertyChanged("ProcessInfo");
            }
        }

        [Order(8)]
        public List<EventTriggerModel> SubEvents { get; private set; } = new List<EventTriggerModel>();

        [Order(9)]
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
                if (EventType == EventType.Mouse && MousePoint.HasValue)
                    return $"X : { MousePoint.Value.X.ToString("0")} Y : {MousePoint.Value.Y.ToString("0") }";
                else if (EventType == EventType.Keyboard)
                    return KeyboardCmd;
                else
                    return "";
            }
        }

        public EventTriggerModel()
        {
            _keyboardCmd = "";
            _eventType = EventType.Image;
        }
        
        public EventTriggerModel(EventTriggerModel obj)
        {
            Index = obj.Index;
            Image = obj.Image;
            EventType = obj.EventType;
            MousePoint = obj.MousePoint;
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
