using System;
using System.Globalization;
using System.Windows.Data;

namespace Macro.Infrastructure.Converter
{
    public class BooleanToValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString().Equals(parameter.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(System.Convert.ToBoolean(value))
            {
                return Enum.Parse(targetType, parameter.ToString(), true);
            }

            return Binding.DoNothing;
        }
    }
}
