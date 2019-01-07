using System;
using Utils.Infrastructure;

namespace Macro.Models
{
    [Serializable]
    public class ProcessInfo
    {
        public string ProcessName { get; set; }
        public Rect Position { get; set; } = new Rect();
    }
}
