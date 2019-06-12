using System;
using System.Threading.Tasks;

namespace Utils.Extensions
{
    public static class TaskExtensions
    {
        public static Task Finally(this Task @this, Action<object> action, object state)
        {
            try
            {
                switch (@this.Status)
                {
                    case TaskStatus.Faulted:
                    case TaskStatus.Canceled:
                        action(state);
                        return @this;
                    case TaskStatus.RanToCompletion:
                        action(state);
                        return Task.CompletedTask;
                    default:
                        return @this.ContinueWith((task) =>
                        {
                            action(state);
                        }, TaskContinuationOptions.ExecuteSynchronously);
                }
            }
            catch (Exception ex)
            {
                return Task.FromException(ex);
            }
        }
        public static Task Then<T1, T2, T3>(this Task @this, Func<T1, T2, T3, Task> func, T1 args1, T2 args2, T3 args3)
        {
            try
            {
                switch (@this.Status)
                {
                    case TaskStatus.Faulted:
                    case TaskStatus.Canceled:
                        return @this;
                    default:
                        return func(args1, args2, args3);
                }
            }
            catch (Exception ex)
            {
                return Task.FromException(ex);
            }
        }
    }
}
