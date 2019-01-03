using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Utils.Infrastructure;

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

        public static List<MonitorInfo> MonitorInfo()
        {
            var monitors = new List<MonitorInfo>();
            int index = 0;
            var factor = NativeHelper.GetSystemDpi();

            NativeHelper.MonitorEnumDelegate callback = new NativeHelper.MonitorEnumDelegate((IntPtr hMonitor, IntPtr hdcMonitor, ref Rect rect, int data) =>
            {
                rect.Right = rect.Right;
                rect.Bottom = rect.Bottom;

                monitors.Add(new MonitorInfo()
                {
                    Rect = rect,
                    Index = index++,
                    FactorX = factor.X / DefaultDPI,
                    FactorY = factor.Y / DefaultDPI,

                });
                return true;
            });
            NativeHelper.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, 0);
            return monitors;
        }
        public static Bitmap Capture(MonitorInfo monitor, Rect rect)
        {
            try
            {
                Bitmap bmp = new Bitmap((int)Math.Truncate(rect.Width * monitor.FactorX), (int)Math.Truncate(rect.Height * monitor.FactorY));
                using (var g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(monitor.Rect.Left, monitor.Rect.Top,
                        (int)Math.Truncate(rect.Left * - 1 * monitor.FactorX), (int)Math.Truncate(rect.Top * -1 * monitor.FactorY),
                        new Size(monitor.Rect.Width, monitor.Rect.Height),
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
        public static bool ProcessCapture(Process process, out Bitmap bmp)
        {
            try
            {
                IntPtr hWnd = process.MainWindowHandle;
                var factor = NativeHelper.GetSystemDpi();
                if (hWnd == IntPtr.Zero)
                {
                    bmp = null;
                    return false;
                }
                Rect rect = new Rect();
                NativeHelper.GetWindowRect(hWnd, ref rect);
                if (rect.Width == 0 || rect.Height == 0)
                {
                    bmp = null;
                    return false;
                }
                var factorX = factor.X / DefaultDPI;
                var factorY = factor.Y / DefaultDPI;

                bmp = new Bitmap((int)Math.Truncate(rect.Width * factorX), (int)Math.Truncate(rect.Height * factorY), PixelFormat.Format32bppArgb);
                using (var gfxBmp = Graphics.FromImage(bmp))
                {
                    IntPtr hdcBitmap = gfxBmp.GetHdc();
                    NativeHelper.PrintWindow(hWnd, hdcBitmap, 0x02);
                    gfxBmp.ReleaseHdc(hdcBitmap);
                    gfxBmp.Dispose();
                }
                return true;
            }
            catch(Exception ex)
            {
                LogHelper.Warning(ex.Message);
                bmp = null;
                return false;
            }
        }
    }
}
