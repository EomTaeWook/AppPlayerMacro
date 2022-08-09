using KosherUtils.Log;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Utils.Infrastructure
{
    public class TaskHelper
    {
        public static async Task<bool> TokenCheckDelayAsync(int millisecondsDelay, CancellationToken token)
        {
            try
            {
                if (millisecondsDelay > 0)
                {
                    await Task.Delay(millisecondsDelay, token);
                }
            }
            catch (TaskCanceledException ex)
            {
                LogHelper.Error(ex.Message);
            }
            catch (AggregateException ex)
            {
                LogHelper.Error(ex.Message);
            }
            return !token.IsCancellationRequested;
        }
    }
}
