using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro.Models
{
    internal class ProcessResultModel
    {
        public bool Excuted { get; set; }
        public EventTriggerModel NextExcuteModel { get; set; }
    }
}
