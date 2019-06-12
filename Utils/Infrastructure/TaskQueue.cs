using System;
using System.Threading;
using System.Threading.Tasks;
using Utils.Extensions;

namespace Utils.Infrastructure
{

    public class TaskQueue
    {
        private object _lock;
        private readonly int _maxSize;
        private int _size;
        private Task _lastQueuedTask;
        private volatile bool _drained;
        public TaskQueue() : this(int.MaxValue)
        {
        }
        public TaskQueue(int maxSize)
        {
            _lock = new object();
            _lastQueuedTask = Task.CompletedTask;
            if (maxSize <= 0)
            {
                throw new InvalidOperationException();
            }
            _maxSize = maxSize;
        }

        public bool IsDrained => _drained;

        public Task Enqueue(Func<object, Task> func, object state)
        {
            lock(_lock)
            {
                if (IsDrained)
                {
                    return _lastQueuedTask;
                }
                if (Interlocked.Increment(ref _size) >= _maxSize)
                {
                    Interlocked.Decrement(ref _size);
                    return null;
                }
                var newTask = _lastQueuedTask.Then( (next, s, q) => q.InvokeNext(next, s), func, state, this);
                return _lastQueuedTask = newTask;
            }
        }
        public Task InvokeNext(Func<object, Task> func, object nextState)
        {
            return func(nextState).Finally((q) => { ((TaskQueue)q).Dequeue(); }, this);
        }
        public Task Drain()
        {
            lock (_lock)
            {
                _drained = true;
                return _lastQueuedTask;
            }
        }
        public void Clear()
        {
            _drained = false;
        }
        public Task Enqueue(Func<Task> func)
        {
            return Enqueue(f => ((Func<Task>)f).Invoke(), func);
        }
        private void Dequeue()
        {
            Interlocked.Decrement(ref _size);
        }
    }
}
