using Macro.Extensions;
using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;
namespace Macro.Infrastructure.Converter
{
    [ValueConversion(typeof(Bitmap), typeof(System.Windows.Media.ImageSource))]
    public class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            return ((Bitmap)value).ToBitmapSource();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
