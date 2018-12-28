using System.ComponentModel;
using System.Drawing;
using Point = System.Windows.Point;

namespace Macro.Models
{
    public class ConfigEventModel
    {
        public ConfigEventModel()
        {
        }
        public ConfigEventModel(ConfigEventModel obj)
        {
            Image = obj.Image;
            EventType = obj.EventType;
            MousePoint = obj.MousePoint;
            KeyBoardCmd = obj.KeyBoardCmd;
            ProcessName = obj.ProcessName;
        }
        public Bitmap Image { get; set; }

        public EventType EventType { get; set; }

        public Point? MousePoint { get; set; }

        public string KeyBoardCmd { get; set; } = "";

        public string ProcessName { get; set; } = "";

        public override int GetHashCode()
        {
            return Image ? .GetHashCode() ?? new object().GetHashCode() +
                EventType.GetHashCode() +
                MousePoint ? .GetHashCode() ?? new Point().GetHashCode() +
                KeyBoardCmd.GetHashCode() +
                ProcessName.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return GetHashCode().Equals(obj.GetHashCode());
        }
    }
}
