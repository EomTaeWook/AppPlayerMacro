using Macro.Models;
using System.Threading;
using Macro.Extensions;
using System;
using Utils;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Macro.Infrastructure
{
    public class ProcessManager
    {
        public static void Start()
        {
            ObjectExtensions.GetInstance<ProcessManager>().Process();
        }
        public static void Stop()
        {
            ObjectExtensions.GetInstance<ProcessManager>().Drain();
        }
        public static void AddJob(Func<Task> taskFunc)
        {
            ObjectExtensions.GetInstance<ProcessManager>().Jobs.Add(taskFunc);
        }

        private readonly IConfig _config;
        private CancellationTokenSource cts;
        private Thread _workThread;
        public List<Func<Task>> Jobs { get; private set; }

        public ProcessManager(IConfig config)
        {
            cts = new CancellationTokenSource();
            _config = config;
            Jobs = new List<Func<Task>>();

            AppDomain.CurrentDomain.ProcessExit += (s, e) =>
            {
                Stop();
            };
        }
        private void Drain()
        {
            if (_workThread != null)
            {
                cts.Cancel();
                _workThread.Join();
                _workThread = null;
                cts = null;
            }
        }
        private void Process()
        {
            Drain();

            cts = new CancellationTokenSource();
            _workThread = new Thread(OnProcess);
            _workThread.Start();
        }
        private void OnProcess()
        {
            while(!cts.IsCancellationRequested)
            {
                foreach (var job in Jobs)
                {
                    if (!job().IsCompleted)
                    {
                        break;
                    }
                }
                Thread.Sleep(_config.Period);
            }
        }
    }
}
