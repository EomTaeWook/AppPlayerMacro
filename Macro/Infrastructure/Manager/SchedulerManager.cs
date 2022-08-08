using KosherUtils.Coroutine;
using KosherUtils.Framework;
using Macro.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utils;
using Utils.Infrastructure;

namespace Macro.Infrastructure.Manager
{
    public class SchedulerManager : Singleton<SchedulerManager>
    {
        private CancellationTokenSource cts;
        private int period = ConstHelper.MinPeriod;
        public SchedulerManager()
        {

        }
        public bool IsRunning()
        {
            if(cts == null)
            {
                return false;
            }
            return cts.IsCancellationRequested == false;
        }
        public void Start()
        {
            if(cts != null)
            {
                Stop();
            }
            TaskBuilder.Build(Update, out CancellationTokenSource token);
            cts = token;
        }
        public void Stop()
        {
            if(cts != null)
            {
                cts.Cancel();
                cts = null;
            }
        }
        public void Update()
        {
            long previousTick = DateTime.Now.Ticks;
            var args = new UpdatedTimeArgs();
            while (cts.IsCancellationRequested == false)
            {
                var currentTime = DateTime.Now.Ticks;
                args.DeltaTime = (float)TimeSpan.FromTicks(currentTime - previousTick).TotalSeconds;
                previousTick = currentTime;
                NotifyHelper.InvokeNotify(NotifyEventType.UpdatedTime, args);
                Task.Delay(period);
            }
        }
    }
}
