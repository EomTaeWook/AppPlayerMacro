using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Drawing;
using Point = OpenCvSharp.Point;

namespace Macro.Infrastructure
{
    public class OpenCVHelper
    {
        public static int Search(Bitmap source, Bitmap target)
        {
            return Search(source, target, out System.Windows.Point location);
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
#if DEBUG
            using (var g = Graphics.FromImage(source))
            {
                using (var pen = new Pen(Color.Red, 2))
                {
                    g.DrawRectangle(pen, new Rectangle() { X = (int)location.X, Y = (int)location.Y, Width = target.Width, Height = target.Height });
                }
            }
#endif
            return Convert.ToInt32(max * 100);
        }
    }
}
