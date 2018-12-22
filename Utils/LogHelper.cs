using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public class LogHelper
    {
        private static readonly NLog.ILogger _logger = NLog.LogManager.GetCurrentClassLogger();
        public static void Error(string message, [CallerMemberName] string callerName = "")
        {
            _logger.Error(message, callerName);
        }
        public static void Info(string message, [CallerMemberName] string callerName = "")
        {
            _logger.Info(message, callerName);
        }
        public static void Warning(string message, [CallerMemberName] string callerName = "")
        {
            _logger.Warn(message, callerName);
        }
    }
}
