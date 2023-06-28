using Dignus.Framework;
using Dignus.Log;
using Macro.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using Utils.Infrastructure;

namespace Macro.Infrastructure.Manager
{
    public class SchedulerManager : Singleton<SchedulerManager>
    {
        private CancellationTokenSource cts;
        private CancellationToken cancellationToken = CancellationToken.None;
        public SchedulerManager()
        {
        }
        public bool IsRunning()
        {
            return cancellationToken != CancellationToken.None;
        }
        public Task Start()
        {
            if(IsRunning() == true)
            {
                LogHelper.Error($"scheduler is already running!");
            }
            cts = new CancellationTokenSource();
            cancellationToken = cts.Token;
            return TaskBuilder.Build(async ()=> { await UpdateAsync(); });
        }
        public void Stop()
        {
            cts.Cancel();
            cts.Dispose();
            cancellationToken = CancellationToken.None;
        }
        public async Task UpdateAsync()
        {
            long previousTick = DateTime.Now.Ticks;
            var args = new UpdatedTimeArgs();
            while (IsRunning() == true)
            {
                var currentTime = DateTime.Now.Ticks;
                args.DeltaTime = (float)TimeSpan.FromTicks(currentTime - previousTick).TotalSeconds;
                previousTick = currentTime;

                NotifyHelper.InvokeNotify(NotifyEventType.UpdatedTime, args);

                await Task.Delay(33);
            }
        }
    }
}
