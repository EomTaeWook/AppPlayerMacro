using System.Runtime.InteropServices;

namespace Utils.Infrastructure
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DWMThumbnailProperties
    {
        public uint Flags;
        public Rect Destination;
        public Rect Source;
        public byte Opacity;
        public bool Visible;
        public bool SourceClientAreaOnly;
    }
}
