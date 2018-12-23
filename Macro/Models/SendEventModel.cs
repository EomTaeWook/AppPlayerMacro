using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Macro.Models
{
    public class SendEventModel
    {
        public ImageBrush Image { get; set; }

        public Point MousePoint { get; set; }

        public string KeyBoard { get; set; }
    }
}
