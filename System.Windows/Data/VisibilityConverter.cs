using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Codify.System.Windows.Data
{
    public sealed class VisibilityConverter : StaticInstanceBase<VisibilityConverter>, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = BooleanConverter.Instance.Convert(value, targetType, parameter, culture);
            return result == DependencyProperty.UnsetValue ? result :
                result is true ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}