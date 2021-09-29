using System;

namespace Macro.Infrastructure
{
    internal class Current
    {
        public static readonly int Major = 2;
        public static readonly int Minor = 5;
        public static readonly int Build = 4;
    }

    public class Version : IComparable
    {
        public int Major { get; set; }
        public int Minor { get; set; }
        public int Build { get; set; }
        public int CompareTo(object obj)
        {
            Version other = obj as Version;
            return (Major * 1000 + Minor * 100 + Build).CompareTo(other.Major * 1000 + other.Minor * 100 + other.Build);
        }
        public static Version CurrentVersion
        {
            get => new Lazy<Version>(() => 
            {
                return new Version()
                {
                    Major = Current.Major,
                    Minor = Current.Minor,
                    Build = Current.Build
                };
            }).Value;
        }
    }
}
