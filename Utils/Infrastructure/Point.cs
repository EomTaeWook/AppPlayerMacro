using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Utils.Infrastructure
{
    [StructLayout(LayoutKind.Sequential)]
    public struct InterPoint
    {
        public int X;
        public int Y;

        public static implicit operator Point(InterPoint point)
        {
            return new Point(point.X, point.Y);
        }
    }
}
