using System;

namespace Patcher.Infrastructure
{
    public class Version : IComparable
    {
        public int Major { get; set; }
        public int Minor { get; set; }
        public int Build { get; set; }
        public Version()
        {
        }
        public Version(int major, int minor, int build)
        {
            Major = major;
            Minor = minor;
            Build = build;
        }
        public int CompareTo(object obj)
        {
            Version other = obj as Version;
            return (Major * 1000 + Minor * 100 + Build).CompareTo(other.Major * 1000 + other.Minor * 100 + other.Build);
        }
        public string ToVersionString()
        {
            return $"{Major}.{Minor}.{Build}";
        }
    }
}
