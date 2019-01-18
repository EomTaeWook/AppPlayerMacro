using Utils.Infrastructure;

namespace Utils.Extensions
{
    public static class RectExtensions
    {
        public static bool IsContain(this Rect source, Rect other)
        {
            return source.Left <= other.Right && source.Right >= other.Left && source.Top <= other.Bottom && source.Bottom >= other.Top;
        }
    }
}
