using Microsoft.Win32.SafeHandles;
using System;
using System.Drawing;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Macro.Extensions
{
    public static class Image
    {
        public static BitmapSource ToBitmapSource(this Bitmap source)
        {
            using (var handle = new SafeHBitmapHandle(source))
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle.DangerousGetHandle(),
                    IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
        }
        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr obj);

        private sealed class SafeHBitmapHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            [SecurityCritical]
            public SafeHBitmapHandle(Bitmap bitmap) : base(true)
            {
                SetHandle(bitmap.GetHbitmap());
            }

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            protected override bool ReleaseHandle()
            {
                return DeleteObject(handle) > 0;
            }
        }
    }
}
