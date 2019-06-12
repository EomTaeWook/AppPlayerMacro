using NLog;
using System;
using System.Diagnostics;
using System.Linq;
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
        public static void Error(Exception ex)
        {
            var st = new StackTrace(ex, true);
            var frames = st.GetFrames().Select(f =>
            new
            {
                FileName = f.GetFileName(),
                LineNumber = f.GetFileLineNumber(),
                ColumnNumber = f.GetFileColumnNumber(),
                Method = f.GetMethod(),
                Class = f.GetMethod().DeclaringType,
            });
            foreach (var frame in frames)
            {
                if (string.IsNullOrEmpty(frame.FileName))
                    continue;
                _logger.Error($"[ {frame.FileName} : {frame.LineNumber} ] [ {ex.Message} ]");
                _logger.Error(frame.FileName, frame.LineNumber);
            }
        }
        public static void Warning(Exception ex)
        {
            var st = new StackTrace(ex, true);
            var frames = st.GetFrames().Select(f =>
            new
            {
                FileName = f.GetFileName(),
                LineNumber = f.GetFileLineNumber(),
                ColumnNumber = f.GetFileColumnNumber(),
                Method = f.GetMethod(),
                Class = f.GetMethod().DeclaringType,
            });
            foreach (var frame in frames)
            {
                if (string.IsNullOrEmpty(frame.FileName))
                    continue;
                _logger.Warn($"[ {frame.FileName} : {frame.LineNumber} ] [ {ex.Message} ]");
            }
        }
        public static void Debug(string message, [CallerLineNumber] int callerLine = 0, [CallerFilePath] string className = "")
        {
            if (string.IsNullOrWhiteSpace(className))
            {
                throw new ArgumentException("message", nameof(className));
            }
            _logger.Debug(message, callerLine);
        }
    }
}
