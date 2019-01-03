using System;
using System.Threading.Tasks;

namespace Macro.Extensions
{
    public static class TaskExtensions
    {
        public static Task ExecuteSynchronously(this Task task, Action<object> next, object state)
        {
            var result = new TaskCompletionSource<Task>();
            task.ContinueWith(r => 
            {
                try
                {
                    next(state);
                    result.SetResult(Task.CompletedTask);
                }
                catch (Exception ex)
                {
                    result.SetException(ex);
                }
            }, TaskContinuationOptions.ExecuteSynchronously);

            return result.Task;
        }
        public static Task Then<T1, T2, T3>(this Task task, Func<T1, T2, T3, Task> next, T1 arg1, T2 arg2, T3 arg3)
        {
            switch (task.Status)
            {
                case TaskStatus.Faulted:
                case TaskStatus.Canceled:
                    return task;
                case TaskStatus.RanToCompletion:
                    return next(arg1, arg2, arg3);
                default:
                    return next(arg1, arg2, arg3);
            }
        }
        public static Task Finally(this Task task, Action<object> next, object state)
        {
            try
            {
                switch (task.Status)
                {
                    case TaskStatus.Faulted:
                    case TaskStatus.Canceled:
                        next(state);
                        return task;
                    case TaskStatus.RanToCompletion:
                        next(state);
                        return Task.CompletedTask;
                    default:
                        return task.ExecuteSynchronously(next, state);
                }
            }
            catch (Exception ex)
            {
                return Task.FromException(ex);
            }
        }
    }
}
