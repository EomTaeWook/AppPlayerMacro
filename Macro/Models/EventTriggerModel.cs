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

        public event PropertyChangedEventHandler PropertyChanged;

        public int Index { get; set; }

        public Bitmap Image { get; set; }

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

        public MonitorInfo MonitorInfo { get; set; }

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

        public ProcessInfo ProcessInfo
        {
            get => _processInfo;
            set
            {
                _processInfo = value;
                OnPropertyChanged("ProcessInfo");
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
        }

        public List<EventTriggerModel> SubEvents { get; private set; } = new List<EventTriggerModel>();

        public EventTriggerModel(EventTriggerModel obj)
        {
            Index = obj.Index;
            Image = obj.Image;
            EventType = obj.EventType;
            MousePoint = obj.MousePoint;
            KeyboardCmd = obj.KeyboardCmd;
            ProcessInfo = obj.ProcessInfo;
            MonitorInfo = obj.MonitorInfo;
            SubEvents = obj.SubEvents;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
