using System;
using System.ComponentModel;
using System.Drawing;
using Utils.Infrastructure;
using Point = System.Windows.Point;

namespace Macro.Models
{
    public class ConfigEventModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Point? _mousePoint;
        private string _keyboardCmd;
        private EventType _eventType;

        public Bitmap Image { get; set; }

        public int Index { get; set; }
        public MonitorInfo MonitorInfo { get; set; }

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
            get => _mousePoint;
            set
            {
                _mousePoint = value;
                OnPropertyChanged("MousePoint");
                OnPropertyChanged("Desc");
            }
        }

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
        public string ProcessName { get; set; } = "";
        public string Desc
        {
            get
            {
                if (EventType == EventType.Mouse && MousePoint.HasValue)
                    return $"X : { MousePoint.Value.X.ToString("0.0##")} Y : {MousePoint.Value.Y.ToString("0.0##") }";
                else if (EventType == EventType.Keyboard)
                    return KeyboardCmd;
                else
                    return "";
            }
        }

        public ConfigEventModel()
        {
            _keyboardCmd = "";
        }
        public ConfigEventModel(ConfigEventModel obj)
        {
            Index = obj.Index;
            Image = obj.Image;
            EventType = obj.EventType;
            MousePoint = obj.MousePoint;
            KeyboardCmd = obj.KeyboardCmd;
            ProcessName = obj.ProcessName;
            MonitorInfo = obj.MonitorInfo;
        }
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
