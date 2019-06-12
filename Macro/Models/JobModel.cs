using System.Threading;

namespace Macro.Models
{
    public class JobModel
    {
        public CancellationToken Token { get; set; }

        public EventTriggerModel Model { get; set; }
    }
}
