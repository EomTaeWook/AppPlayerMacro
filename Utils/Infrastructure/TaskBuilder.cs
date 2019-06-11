using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Utils.Infrastructure
{
    public class TaskBuilder
    {
        public static Task Build(Action action)
        {
            return Task.Run(action);
        }
        public static Task Build(Action action, out CancellationToken token)
        {
            token = new CancellationTokenSource().Token;

            return Task.Run(action, token);
        }
    }
}
