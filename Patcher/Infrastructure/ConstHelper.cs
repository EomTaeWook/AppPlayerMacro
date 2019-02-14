using System.IO;

namespace Patcher.Infrastructure
{
    public class ConstHelper
    {
        public static readonly string TempPath = Path.GetTempPath() + "Macro";
        public static readonly string TempBackupPath = $@"{ TempPath }\backup\";
    }
}
