using System.ComponentModel;
using System.Drawing;
using Point = System.Windows.Point;

namespace Macro.Models
{
    public class ConfigEventModel : INotifyPropertyChanged
    {
        private Point? _mousePoint;
        private string _keyboardCmd;
        private EventType _eventType;
        public ConfigEventModel()
        {
            //_mousePoint = new Point();
            _keyboardCmd = "";
        }
        public ConfigEventModel(ConfigEventModel obj)
        {
            Image = obj.Image;
            EventType = obj.EventType;
            MousePoint = obj.MousePoint;
            KeyboardCmd = obj.KeyboardCmd;
            ProcessName = obj.ProcessName;
        }

        public Bitmap Image { get; set; }

        public int Index { get; set; }

        public EventType EventType
        {
            get => _eventType;
            set
            {
                _eventType = value;
                OnPropertyChanged("Desc");
            }
        }

        public Point? MousePoint {
            get =>_mousePoint;
            set
            {
                _mousePoint = value;
                OnPropertyChanged("Desc");
            }
        }

        public string KeyboardCmd
        {
            get => _keyboardCmd;
            set
            {
                _keyboardCmd = value;
                OnPropertyChanged("Desc");
            }
        }
        public string ProcessName { get; set; } = "";
        public string Desc
        {
            get
            {
                if (EventType == EventType.Mouse && MousePoint.HasValue)
                    return $"X : { MousePoint.Value.X } Y : {MousePoint.Value.Y }";
                else if (EventType == EventType.Keyboard)
                    return KeyboardCmd;
                else
                    return "";
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
