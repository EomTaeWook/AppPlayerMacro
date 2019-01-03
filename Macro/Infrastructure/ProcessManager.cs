using Macro.Extensions;
using Macro.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Macro.Infrastructure
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
        public static void AddJob(Func<Task> taskFunc)
        {
            ObjectExtensions.GetInstance<ProcessManager>().Jobs.Add(taskFunc);
        }

        public List<Func<Task>> Jobs { get; private set; }

        private readonly IConfig _config;
        private CancellationTokenSource _cts;
        private Thread _workThread;
        private TaskCompletionSource<bool> _tcs;

        public ProcessManager(IConfig config)
        {
            _config = config;
            Jobs = new List<Func<Task>>();

            AppDomain.CurrentDomain.ProcessExit += (s, e) =>
            {
                Stop();
            };
        }
        private Task Drain()
        {
            if (_workThread != null)
            {
                _cts.Cancel();
                _tcs.Task.Wait();
                _workThread = null;
                _cts.Dispose();
                _cts = null;
            }
            return Task.CompletedTask;
        }
        private Task Process()
        {
            Drain().Wait();
            _tcs = new TaskCompletionSource<bool>();
            _cts = new CancellationTokenSource();
            _workThread = new Thread(OnProcess);
            _workThread.Start();
            return Task.CompletedTask;
        }
        private void OnProcess()
        {
            while(!_cts.IsCancellationRequested)
            {
                foreach (var job in Jobs)
                {
                    if (!job().IsCompleted)
                    {
                        break;
                    }
                }
                if (_cts.Token.WaitHandle.WaitOne(_config.Period))
                    break;
            }
            _tcs.SetResult(true);
        }
    }
}
