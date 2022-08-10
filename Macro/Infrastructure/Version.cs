using System;

namespace Macro.Infrastructure
{
    internal class Current
    {
        public static readonly int Major = 2;
        public static readonly int Minor = 5;
        public static readonly int Build = 4;
    }

    public class Version : Utils.Infrastructure.Version
    {
        public Version()
        {

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
