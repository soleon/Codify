using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Codify.Windows.Converters
{
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolValue = BooleanConverter.ToBoolean(value, targetType, parameter, culture);
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}