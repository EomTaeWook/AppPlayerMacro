using Macro.Models;

namespace Macro.Infrastructure
{
    public class RepeatInfo
    {
        public RepeatType RepeatType { get; set; } = RepeatType.Once;
        public ushort Count { get; set; } = 1;
    }
}
