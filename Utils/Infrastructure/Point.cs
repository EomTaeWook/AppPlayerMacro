using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Utils.Infrastructure
{
    [StructLayout(LayoutKind.Sequential)]
    public struct InterSize
    {
        public int X;
        public int Y;
    }

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

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int Left { get; set; }
        public int Top { get; set; }
        public int Right { get; set; }
        public int Bottom { get; set; }
        public int Width { get => Right - Left; }
        public int Height { get => Bottom - Top; }

        public static Rect operator -(Rect r1, Rect r2)
        {
            return new Rect()
            {
                Left = r1.Left - r2.Left,
                Right = r1.Right - r2.Right,
                Bottom = r1.Bottom - r2.Bottom,
                Top = r1.Top - r2.Top
            };
        }
        public static Rect operator +(Rect r1, Rect r2)
        {
            return new Rect()
            {
                Left = r1.Left + r2.Left,
                Right = r1.Right + r2.Right,
                Bottom = r1.Bottom + r2.Bottom,
                Top = r1.Top + r2.Top
            };
        }
    }
}
