using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int Left { get; set; }
        public int Top { get; set; }
        public int Right { get; set; }
        public int Bottom { get; set; }
        public int Width { get => Right - Left; }
        public int Height { get => Bottom - Top; }
        public Rect(Rect rect)
        {
            Left = rect.Left;
            Top = rect.Top;
            Right = rect.Right;
            Bottom = rect.Bottom;
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    struct MonitorInfo
    {
        public uint Size;
        public Rect Monitor;
        public Rect Work;
        public uint Flags;
    }

    public class CaptureHelper
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

        [DllImport("user32.dll")]
        private static extern IntPtr ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern int SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr rect, MonitorEnumDelegate callback, int data);
        private delegate bool MonitorEnumDelegate(IntPtr desktop, IntPtr hdc, ref Rect rect, int data);
        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hmon, ref MonitorInfo mi);

        public static Rect MonitorSize()
        {
            List<MonitorInfo> infos = new List<MonitorInfo>();
            MonitorEnumDelegate callback = new MonitorEnumDelegate((IntPtr desktop, IntPtr hdc, ref Rect rect, int data) =>
            {
                MonitorInfo info = new MonitorInfo();
                info.Size = (uint)Marshal.SizeOf(info);
                if(GetMonitorInfo(desktop, ref info))
                {
                    infos.Add(info);
                    return true;
                }
                else
                {
                    return false;
                }
            });
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, 0);
            var monitor = new Rect();
            foreach(var info in infos)
            {
                if (monitor.Left > info.Monitor.Left)
                    monitor.Left = info.Monitor.Left;
                if (monitor.Right < info.Monitor.Right)
                    monitor.Right = info.Monitor.Right;
                if (monitor.Bottom < info.Monitor.Bottom)
                    monitor.Bottom = info.Monitor.Bottom;
                if (monitor.Top > info.Monitor.Top)
                    monitor.Top = info.Monitor.Top;
            }
            return monitor;
        }
        public static Bitmap Capture(Rect rect)
        {
            try
            {
                var monitor = MonitorSize();
                Bitmap bmp = new Bitmap(rect.Width, rect.Height);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(monitor.Left, monitor.Top,  (rect.Left * -1), (rect.Top * -1),
                                    new Size(monitor.Width, monitor.Height),
                                    CopyPixelOperation.SourceCopy);
                }
                return bmp;
            }
            catch (Exception ex)
            {
                LogHelper.Warning(ex.Message);
                return null;
            }            
        }
    }
    
}
