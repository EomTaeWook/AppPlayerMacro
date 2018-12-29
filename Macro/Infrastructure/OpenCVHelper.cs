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
        public static double Search(Bitmap search)
        {
            var screen = OpenCvSharp.Extensions.BitmapConverter.ToMat(search);
            var find = OpenCvSharp.Extensions.BitmapConverter.ToMat(search);
            var result = screen.MatchTemplate(find, TemplateMatchModes.CCoeffNormed);
            Cv2.MinMaxLoc(result, out double min, out double max);
            return max;
        }
    }
}
