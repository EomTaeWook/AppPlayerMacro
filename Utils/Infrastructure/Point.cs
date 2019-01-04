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
}
