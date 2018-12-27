using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

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

    public class CaptureHelper
    {
        private const float DefaultDPI = 96.0F;

        public static Rect MonitorSize()
        {
            List<Rect> infos = new List<Rect>();
            NativeHelper.MonitorEnumDelegate callback = new NativeHelper.MonitorEnumDelegate((IntPtr hMonitor, IntPtr hdcMonitor, ref Rect rect, int data) =>
            {
                NativeHelper.GetDpiForMonitor(hMonitor, DpiType.Effective, out uint dpiX, out uint dpiY);
                var factorX = dpiX / DefaultDPI;
                var factorY = dpiY / DefaultDPI;
                rect.Right = (int)(rect.Right * factorX);
                rect.Bottom = (int)(rect.Bottom * factorY);
                infos.Add(new Rect(rect));
                return true;
            });
            NativeHelper.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, 0);
            var monitor = new Rect();
            foreach(var info in infos)
            {
                if (monitor.Left > info.Left)
                    monitor.Left = info.Left;
                if (monitor.Right < info.Right)
                    monitor.Right = info.Right;
                if (monitor.Bottom < info.Bottom)
                    monitor.Bottom = info.Bottom;
                if (monitor.Top > info.Top)
                    monitor.Top = info.Top;
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
                    g.CopyFromScreen(monitor.Left, monitor.Top, (rect.Left * -1), (rect.Top * -1),
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
