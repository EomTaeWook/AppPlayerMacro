using System.Collections.Generic;

namespace Patcher.Infrastructure
{
    internal class Dependency
    {
        public static List<string> List { get; } = new List<string>()
        {
            "Newtonsoft.Json.dll",
            "NLog.dll",
            "Utils.dll",
            "ControlzEx.dll",
            "MahApps.Metro.dll",
            "System.Windows.Interactivity.dll",
            "WindowsBase.dll",
            "KosherUtils.dll"
        };
    }
}
