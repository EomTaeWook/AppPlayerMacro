namespace Patcher.Infrastructure
{
    public class ConstHelper
    {
        public static readonly string[] ExcludeExtension = new string[]
        {
            ".config",
            ".pdb",
        };

        public static readonly string TempBackupPath = Utils.ConstHelper.TempBackupPath;
        public static readonly string TempPath = Utils.ConstHelper.TempPath;

        public static readonly string PatchUrl = @"https://drive.google.com/uc?export=view&id=1y4DIXlCb4cRqTfQHwgeyyJ0ky4WZBmD_";
        public static readonly string PatchV2Url = @"https://drive.google.com/uc?export=view&id=1Eu12YCMSecEh0g5WP1UfNSy9s6AsQZtH";
        public static readonly string PatchV3Url = @"https://drive.google.com/uc?export=view&id=1AjXI-PpMrjLZzjsmnh5T0ddRe9xpIwtM";
    }
}
