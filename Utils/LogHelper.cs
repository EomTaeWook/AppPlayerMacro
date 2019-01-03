using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Utils
{
    public class LogHelper
    {
        private static readonly NLog.ILogger _logger = NLog.LogManager.GetCurrentClassLogger();
        public static void Error(string message, [CallerMemberName] string callerName = "")
        {
            _logger.Error(message, callerName);
        }
        public static void Debug(string message, [CallerMemberName] string callerName = "")
        {
            Trace.WriteLine(message);
            _logger.Debug(message, callerName);
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
