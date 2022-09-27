using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro.Models
{
    internal class ProcessResult
    {
        public int DelayMillisecond { get; set; }

        public bool Executed { get; set; }

        public EventTriggerModel NextEventModel { get; set; }
    }
}
