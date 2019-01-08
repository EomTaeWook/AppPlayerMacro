using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using Utils.Infrastructure;

namespace Utils
{
    public class CaptureHelper
    {
        public static List<MonitorInfo> MonitorInfo()
        {
            var monitors = new List<MonitorInfo>();
            int index = 0;

            NativeHelper.MonitorEnumDelegate callback = new NativeHelper.MonitorEnumDelegate((IntPtr hMonitor, IntPtr hdcMonitor, ref Rect rect, int data) =>
            {
                rect.Right = rect.Right;
                rect.Bottom = rect.Bottom;

                monitors.Add(new MonitorInfo()
                {
                    Rect = rect,
                    Index = index++,
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
                var factor = NativeHelper.GetSystemDpi();
                var factorX = factor.X / ConstHelper.DefaultDPI;
                var factorY = factor.Y / ConstHelper.DefaultDPI;

                Bitmap bmp = new Bitmap((int)Math.Truncate(rect.Width * factorX), (int)Math.Truncate(rect.Height * factorY));
                using (var g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(monitor.Rect.Left, monitor.Rect.Top,
                        (int)Math.Truncate(rect.Left * - 1 * factorX), (int)Math.Truncate(rect.Top * -1 * factorY),
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
                var factorX = factor.X / ConstHelper.DefaultDPI;
                var factorY = factor.Y / ConstHelper.DefaultDPI;

                bmp = new Bitmap((int)Math.Truncate(rect.Width * factorX), (int)Math.Truncate(rect.Height * factorY), PixelFormat.Format32bppArgb);
                using (var gfxBmp = Graphics.FromImage(bmp))
                {
                    IntPtr hdcBitmap = gfxBmp.GetHdc();
                    NativeHelper.PrintWindow(hWnd, hdcBitmap, 0x02 | 0x03);
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
