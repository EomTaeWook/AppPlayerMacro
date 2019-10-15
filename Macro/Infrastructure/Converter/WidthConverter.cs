using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;

namespace Macro.Infrastructure.Converter
{
    public class DependencyObjectWidthConverter : IValueConverter
    {
        private readonly string _regex = @"^[0-9]*[.]\d{1,2}\*$|^[0-9]\*$";
        public object Convert(object value, Type type, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            if (value is DependencyObject == false)
                return null;
            var @object = value as DependencyObject;

            if (Regex.IsMatch(parameter.ToString(), _regex))
            {
                var objectWidth = (double)@object.GetValue(FrameworkElement.ActualWidthProperty);
                var param = parameter.ToString();
                if (double.TryParse(param.Substring(0, param.Length - 1), out double width))
                    return (int)Math.Truncate(objectWidth * width);
                else
                    return null;
            }
            else
            {
                return parameter;
            }
        }
        public object ConvertBack(object o, Type type, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class WidthConverter : IValueConverter
    {
        private readonly string _regex = @"^[0-9]*[.]\d{1,2}\*$|^[0-9]\*$";
        public object Convert(object value, Type type, object parameter, CultureInfo culture)
        {
            if (value is double == false)
                return value;

            if (Regex.IsMatch(parameter.ToString(), _regex))
            {
                var objectWidth = System.Convert.ToDouble(value);
                var param = parameter.ToString();
                if (double.TryParse(param.Substring(0, param.Length - 1), out double width))
                    return (int)Math.Truncate(objectWidth * width);
                else
                    return null;
            }
            else
            {
                return parameter;
            }
        }
        public object ConvertBack(object o, Type type, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
