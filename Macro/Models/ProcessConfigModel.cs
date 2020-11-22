using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Macro.Models
{
    public class ProcessConfigModel
    {
        public List<Process> Processes { get; set; }
        public CancellationToken Token { get; set; }
        public int ItemDelay { get; set; }
        public int Similarity { get; set; }
        public int DragDelay { get; set; }
        public bool SearchImageResultDisplay { get; set; }
    }
}
