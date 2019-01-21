using Macro.Extensions;
using Macro.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Macro.Infrastructure.Manager
{
    public class ProcessManager
    {
        public static Task Start()
        {
            return ObjectExtensions.GetInstance<ProcessManager>().Process();
        }
        public static Task Stop()
        {
            return ObjectExtensions.GetInstance<ProcessManager>().Drain();
        }
        public static void AddJob(Func<CancellationToken, Task> taskFunc)
        {
            ObjectExtensions.GetInstance<ProcessManager>().Jobs.Add(taskFunc);
        }

        public List<Func<CancellationToken, Task>> Jobs { get; private set; }

        private readonly IConfig _config;
        private CancellationTokenSource _cts;
        private Task _current;
        private volatile int _atomic = 0;

        public ProcessManager(IConfig config)
        {
            _config = config;
            Jobs = new List<Func<CancellationToken, Task>>();

            AppDomain.CurrentDomain.ProcessExit += (s, e) =>
            {
                Stop();
            };
        }
        private Task Drain()
        {
            if(_atomic == 1)
            {
                _cts.Cancel();
                if(_current.IsFaulted)
                    Interlocked.Decrement(ref _atomic);
                return _current;
            }

            return Task.CompletedTask;
        }
        private Task Process()
        {
            if (Interlocked.CompareExchange(ref _atomic, 1, 0) != _atomic)
            {
                _cts = new CancellationTokenSource();
                return _current = Task.Factory.StartNew(OnProcess, _cts);
            }
            return _current;
        }
        private void OnProcess(object state)
        {
            var token = (state as CancellationTokenSource).Token;
            while (!token.IsCancellationRequested)
            {
                foreach (var job in Jobs)
                {
                    var task = Task.Factory.StartNew(()=> { return job.Invoke(token); }, token);
                    task.Wait();
                    if (!task.IsCompleted)
                        break;
                }
                if (token.WaitHandle.WaitOne(_config.Period))
                    break;
            }
            Interlocked.Decrement(ref _atomic);
        }
    }
}
