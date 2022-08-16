using System;
using System.IO;

namespace Patcher.Infrastructure
{
    public class ConstHelper
    {
        public static readonly string TempPath = Path.GetTempPath() + $"Macro\\";
        public static readonly string TempBackupPath = $"{ TempPath }backup\\";
        public static readonly string ShadowCopyPath = $"{ Environment.CurrentDirectory }shadow\\";

        public static readonly string PatchUrl = @"https://drive.google.com/uc?export=view&id=1y4DIXlCb4cRqTfQHwgeyyJ0ky4WZBmD_";
        public static readonly string PatchV2Url = @"https://drive.google.com/uc?export=view&id=1Eu12YCMSecEh0g5WP1UfNSy9s6AsQZtH";
        public static readonly string PatchV3Url = @"https://drive.google.com/uc?export=view&id=1AjXI-PpMrjLZzjsmnh5T0ddRe9xpIwtM";
    }
}
