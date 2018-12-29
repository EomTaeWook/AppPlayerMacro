using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro.Infrastructure
{
    public class OpenCVHelper
    {
        public static int Search(Bitmap source, Bitmap target)
        {
            var sourceMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(source);
            var targetMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(target);
            var match = sourceMat.MatchTemplate(targetMat, TemplateMatchModes.CCoeffNormed);
            Cv2.MinMaxLoc(match, out double min, out double max);
            return Convert.ToInt32(max * 100);
        }
    }
}
