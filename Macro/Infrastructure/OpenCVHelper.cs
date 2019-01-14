using OpenCvSharp;
using System;
using System.Drawing;
using OpenCvSharp.Extensions;
using Utils;
using Point = OpenCvSharp.Point;

namespace Macro.Infrastructure
{
    public class OpenCVHelper
    {
        public static int Search(Bitmap source, Bitmap target)
        {
            var sourceMat = BitmapConverter.ToMat(source);
            var targetMat = BitmapConverter.ToMat(target);

            if (sourceMat.Cols <= targetMat.Cols || sourceMat.Rows <= targetMat.Rows)
                return 0;

            var match = sourceMat.MatchTemplate(targetMat,TemplateMatchModes.CCoeffNormed);
            Cv2.MinMaxLoc(match, out double min, out double max, out Point minLoc, out Point maxLoc);
            return Convert.ToInt32(max * 100);
        }
        public static int Search(Bitmap source, Bitmap target, out System.Windows.Point location)
        {
            var sourceMat = BitmapConverter.ToMat(source);
            var targetMat = BitmapConverter.ToMat(target);
            if (sourceMat.Cols <= targetMat.Cols || sourceMat.Rows <= targetMat.Rows)
            {
                location = new System.Windows.Point();
                return 0;
            }

            var match = sourceMat.MatchTemplate(targetMat, TemplateMatchModes.CCoeffNormed);
            Cv2.MinMaxLoc(match, out double min, out double max, out Point minLoc, out Point maxLoc);
            location = new System.Windows.Point()
            {
                X = maxLoc.X,
                Y = maxLoc.Y
            };
            return Convert.ToInt32(max * 100);
        }
    }
}
