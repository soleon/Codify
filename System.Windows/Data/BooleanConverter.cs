using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Codify.System.Windows.Data
{
    public class BooleanConverter : StaticInstanceBase<BooleanConverter>, IValueConverter
    {
        public object Convert(object value, Type targetType = null, object parameter = null, CultureInfo culture = null)
        {
            if (value == DependencyProperty.UnsetValue)
            {
                return value;
            }

            var isInvert = parameter is string stringValue &&
                           stringValue.Equals("invert", StringComparison.OrdinalIgnoreCase);
            switch (value)
            {
                case bool boolValue:
                    return isInvert ? !boolValue : boolValue;
                case null:
                    return isInvert;
                default:
                {
                    var type = value.GetType();
                    return type.IsValueType
                        ? Activator.CreateInstance(type) == value ? !isInvert : isInvert
                        : !isInvert;
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}