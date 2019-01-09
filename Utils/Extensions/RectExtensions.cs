using Utils.Infrastructure;

namespace Utils.Extensions
{
    public static class RectExtensions
    {
        public static bool IsContain(this Rect source, Rect other)
        {
            return source.Left <= other.Left && source.Right >= other.Right && source.Top <= other.Top && source.Bottom >= other.Bottom;
        }
    }
}
