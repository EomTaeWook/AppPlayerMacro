using Macro.Models;

namespace Macro.Infrastructure
{
    public class RepeatInfo
    {
        public RepeatType RepeatType { get; set; }
        public ushort Count { get; set; } = 1;
    }
}
