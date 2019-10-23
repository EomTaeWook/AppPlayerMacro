using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Drawing;
using Utils;
using Point = OpenCvSharp.Point;
using Rect = Utils.Infrastructure.Rect;

namespace Macro.Infrastructure
{
    public class OpenCVHelper
    {
        public static int Search(Bitmap source, Bitmap target, out System.Windows.Point location, bool isResultDisplay = false)
        {
            var sourceMat = BitmapConverter.ToMat(source);
            var targetMat = BitmapConverter.ToMat(target);
            if (sourceMat.Cols <= targetMat.Cols || sourceMat.Rows <= targetMat.Rows)
            {
                location = new System.Windows.Point();
                return 0;
            }

            var match = sourceMat.MatchTemplate(targetMat, TemplateMatchModes.CCoeffNormed);
            Cv2.MinMaxLoc(match, out _, out double max, out _, out Point maxLoc);

            location = new System.Windows.Point()
            {
                X = maxLoc.X,
                Y = maxLoc.Y
            };
            if(isResultDisplay)
            {
                using (var g = Graphics.FromImage(source))
                {
                    using (var pen = new Pen(Color.Red, 2))
                    {
                        g.DrawRectangle(pen, new Rectangle() { X = (int)location.X, Y = (int)location.Y, Width = target.Width, Height = target.Height });
                    }
                }
            }
            return Convert.ToInt32(max * 100);
        }
        public static Bitmap MakeRoiImage(Bitmap source, Rect rect)
        {
            var sourceMat = BitmapConverter.ToMat(source);
            var roiMat = sourceMat.SubMat(new OpenCvSharp.Rect()
            {
                Left = rect.Left,
                Top = rect.Top,
                Height = rect.Height,
                Width = rect.Width
            });
            var destBitmap = BitmapConverter.ToBitmap(roiMat);
            return destBitmap;
        }

        public static int SearchImagePercentage(Bitmap source, Tuple<double, double ,double> lower, Tuple<double, double, double> upper)
        {
            var sourceMat = BitmapConverter.ToMat(source);
            var colorMat = sourceMat.CvtColor(ColorConversionCodes.RGB2HSV);
            var thresholded = new Mat();
            
            Cv2.InRange(colorMat,
                        new Scalar(lower.Item3, lower.Item1, lower.Item2),
                        new Scalar(upper.Item3, upper.Item1, upper.Item2),
                        thresholded);

            return Cv2.CountNonZero(thresholded) / (source.Width * source.Height);
        }
    }
}
