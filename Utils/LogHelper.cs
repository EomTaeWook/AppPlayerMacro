using NLog;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Utils
{
    public class LogHelper
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        public static void Init()
        {
            var config = new NLog.Config.LoggingConfiguration();
            var logFile = new NLog.Targets.FileTarget("file") { FileName = "Error.log", Layout = "${longdate}|${message}" };
            var logConsole = new NLog.Targets.ConsoleTarget("console");
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logConsole);

            config.AddRule(LogLevel.Warn, LogLevel.Fatal, logFile);
            config.AddRule(LogLevel.Error, LogLevel.Fatal, logFile);
            LogManager.Configuration = config;

            AppDomain.CurrentDomain.ProcessExit += (s, e) => 
            {
                LogManager.Shutdown();
            };
        }
        public static void Error(string message, [CallerLineNumber] int callerLine = 0, [CallerFilePath] string className = "")
        {
            _logger.Error(message, callerLine);
        }
        public static void Debug(string message, [CallerLineNumber] int callerLine = 0, [CallerFilePath] string className = "")
        {
            _logger.Debug(message, callerLine);
        }
        public static void Warning(string message, [CallerLineNumber] int callerLine = 0, [CallerFilePath] string className = "")
        {
            _logger.Warn($"[{Path.GetFileNameWithoutExtension(className)} : {callerLine}]|{message}");
        }
    }
}
