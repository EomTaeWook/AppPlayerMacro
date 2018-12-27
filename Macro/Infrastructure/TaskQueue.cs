using Macro.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Macro.Infrastructure
{
    public class TaskQueue
    {
        private readonly object _lock = new object();
        private Task _lastQueuedTask;
        private volatile bool _drained;
        private readonly int? _maxSize;
        private long _size;
        public TaskQueue()
        {
            _lastQueuedTask = Task.CompletedTask;
        }
        public TaskQueue(int maxSize)
        {
            _maxSize = maxSize;
        }
        public bool IsDrained
        {
            get
            {
                return _drained;
            }
        }
        public Task Enqueue(Func<object, Task> taskFunc, object state)
        {
            lock (_lock)
            {
                if (_drained)
                {
                    return _lastQueuedTask;
                }
                if (_maxSize != null)
                {
                    if (Interlocked.Increment(ref _size) > _maxSize)
                    {
                        Interlocked.Decrement(ref _size);
                        return null;
                    }
                }
                var newTask = _lastQueuedTask.Then((n, ns, q) => q.InvokeNext(n, ns), taskFunc, state, this);
                _lastQueuedTask = newTask;
                return newTask;
            }
        }
        private void Dequeue()
        {
            if (_maxSize != null)
            {
                Interlocked.Decrement(ref _size);
            }
        }
        public Task Enqueue(Func<Task> taskFunc)
        {
            return Enqueue(state => ((Func<Task>)state).Invoke(), taskFunc);
        }
        public Task Drain()
        {
            lock (_lock)
            {
                _drained = true;
                return _lastQueuedTask;
            }
        }
        private Task InvokeNext(Func<object, Task> next, object nextState)
        {
            return next(nextState).Finally(r=> ((TaskQueue)r).Dequeue(), this);
        }
    }
}
