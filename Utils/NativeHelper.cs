using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Utils.Infrastructure;

namespace Utils
{
    public class NativeHelper
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

        [DllImport("user32.dll")]
        public static extern int SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr rect, MonitorEnumDelegate callback, int data);
        public delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect rect, int data);

        [DllImport("user32.dll")]
        public static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int flags);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out InterPoint lpPoint);
        public static Point GetCursorPosition()
        {
            GetCursorPos(out InterPoint point);
            return point;
        }

        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        public static Point GetSystemDpi()
        {
            Point result = new Point();
            IntPtr hDC = GetDC(IntPtr.Zero);
            result.X = GetDeviceCaps(hDC, 0x58);
            result.Y = GetDeviceCaps(hDC, 0x5A);
            ReleaseDC(IntPtr.Zero, hDC);
            return result;
        }

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, SpecialWindowHandles hWndInsertAfter, int x, int y, int width, int height, WindowPosFlags flags);
        public static bool SetWindowPos(IntPtr hWnd, Rect rect)
        {
            return SetWindowPos(hWnd, SpecialWindowHandles.Top, rect.Left, rect.Top, rect.Width, rect.Height, WindowPosFlags.ShowWindow | WindowPosFlags.DoNotActivate | WindowPosFlags.AsynchronousWindowPosition | WindowPosFlags.DoNotCopyBits);
        }

        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hWnd, WindowMessage Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern uint SendInput(uint inputCount, Input[] inputs, int structSize);

        [DllImport("shcore.dll")]
        public static extern bool SetProcessDpiAwareness(PROCESS_DPI_AWARENESS value);

        [DllImport("Shcore.dll")]
        private static extern IntPtr GetDpiForMonitor(IntPtr hMonitor, DpiFlags dpiType, out uint dpiX, out uint dpiY);
        public static Point GetMonitorDPI(IntPtr hMonitor)
        {
            GetDpiForMonitor(hMonitor, DpiFlags.Effective, out uint dpiX, out uint dpiY);
            return new Point((int)dpiX, (int)dpiY);
        }
    }
}
