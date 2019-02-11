using System;

namespace Patcher.Infrastructure
{
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
    }
}
