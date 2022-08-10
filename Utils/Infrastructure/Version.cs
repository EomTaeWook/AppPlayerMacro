using System;

namespace Utils.Infrastructure
{
    public class Version
    {
        public static Version MakeVersion(string versionString)
        {
            var splits = versionString.Split('.');

            return splits.Length != 3 ? null : new Version(int.Parse(splits[0]), int.Parse(splits[1]), int.Parse(splits[2]));
        }
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

        public static bool operator >= (Version a, Version b)
        {
            return a.GetVersionNumber() >= b.GetVersionNumber();
        }
        public static bool operator <=(Version a, Version b)
        {
            return a.GetVersionNumber() <= b.GetVersionNumber();
        }
        public static bool operator > (Version a, Version b)
        {
            return a.GetVersionNumber() > b.GetVersionNumber();
        }
        public static bool operator < (Version a, Version b)
        {
            return a.GetVersionNumber() < b.GetVersionNumber();
        }
        public int GetVersionNumber()
        {
            return Major * 1000 + Minor * 100 + Build;
        }
        public string ToVersionString()
        {
            return $"{Major}.{Minor}.{Build}";
        }
    }
}
