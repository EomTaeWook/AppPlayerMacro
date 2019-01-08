using OpenCvSharp;
using System;
using System.Drawing;
using OpenCvSharp.Extensions;
namespace Macro.Infrastructure
{
    public class OpenCVHelper
    {
        public static int Search(Bitmap source, Bitmap target)
        {
            var sourceMat = BitmapConverter.ToMat(source);
            var targetMat = BitmapConverter.ToMat(target);
            var match = sourceMat.MatchTemplate(targetMat, TemplateMatchModes.CCoeffNormed);
            Cv2.MinMaxLoc(match, out double min, out double max);
            return Convert.ToInt32(max * 100);
        }
    }
}
