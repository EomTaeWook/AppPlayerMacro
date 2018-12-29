using System;
using System.Runtime.InteropServices;

namespace Utils
{
    public enum PROCESS_DPI_AWARENESS
    {
        PROCESS_DPI_UNAWARE = 0,
        PROCESS_SYSTEM_DPI_AWARE = 1,
        PROCESS_PER_MONITOR_DPI_AWARE = 2
    }
    public enum DpiType
    {
        Effective = 0,
        Angular = 1,
        Raw = 2,
    }
    public class NativeHelper
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

        [DllImport("user32.dll")]
        public static extern IntPtr ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern int SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr rect, MonitorEnumDelegate callback, int data);
        public delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect rect, int data);

        [DllImport("user32.dll")]
        public static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int flags);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateRectRgn(int left, int top, int right, int bottom);

        [DllImport("user32.dll")]
        public static extern int GetWindowRgn(IntPtr hWnd, IntPtr hRgn);

        [DllImport("user32.dll")]
        public static extern bool SetProcessDPIAware();

        [DllImport("shcore.dll")]
        public static extern bool SetProcessDpiAwareness(PROCESS_DPI_AWARENESS value);

        [DllImport("shcore.dll")]
        public static extern bool GetProcessDpiAwareness(IntPtr hProcess, out PROCESS_DPI_AWARENESS value);

        [DllImport("shcore.dll")]
        public static extern IntPtr GetDpiForMonitor(IntPtr hMonitor, DpiType dpiType, out uint dpiX, out uint dpiY);
    }
}
