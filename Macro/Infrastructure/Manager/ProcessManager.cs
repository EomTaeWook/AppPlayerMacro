//using Macro.Extensions;
//using Macro.Models;
//using System;
//using System.Collections.Generic;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Macro.Infrastructure.Manager
//{
//    public class ProcessManager
//    {
//        public static Task Start()
//        {
//            return ObjectExtensions.GetInstance<ProcessManager>().Process();
//        }
//        public static Task Stop()
//        {
//            return ObjectExtensions.GetInstance<ProcessManager>().Drain();
//        }
//        public static Task Add(IProcessMessage item)
//        {
//            return ObjectExtensions.GetInstance<ProcessManager>().Enqueue(item);
//        }

//        private readonly IConfig _config;
//        private CancellationTokenSource _cts;
//        private Task _current = Task.CompletedTask;

//        private volatile int _atomic = 0;

//        public ProcessManager() : this(null)
//        {
//        }
//        public ProcessManager(IConfig config)
//        {
//            _config = config;
            
//            AppDomain.CurrentDomain.ProcessExit += (s, e) =>
//            {
//                Stop();
//            };
//        }
//        public Task Enqueue(IProcessMessage item)
//        {
//            return Task.CompletedTask;
//        }
//        private Task Drain()
//        {
//            if(_atomic == 1)
//            {
//                _cts.Cancel();
//                if(_current.IsFaulted)
//                    Interlocked.Decrement(ref _atomic);
//                return _current;
//            }

//            return Task.CompletedTask;
//        }
//        private Task Process()
//        {
//            if (Interlocked.CompareExchange(ref _atomic, 1, 0) != _atomic)
//            {
//                _cts = new CancellationTokenSource();
//                return _current = Task.Run(() => { OnProcess(_cts); });
//            }
//            return _current;
//        }
//        private async void OnProcess(object state)
//        {
//            var token = (state as CancellationTokenSource).Token;
//            while (!token.IsCancellationRequested)
//            {
                
//                //foreach (var job in Jobs)
//                //{
//                //    var task = Task.Run(()=> { return job.Invoke(token); }, token);
//                //    await task;
//                //    if (!task.IsCompleted)
//                //        break;
//                //}
//                if (token.WaitHandle.WaitOne(_config.Period))
//                    break;
//            }
//            Interlocked.Decrement(ref _atomic);
//        }
//    }
//}
