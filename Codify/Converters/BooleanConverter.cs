using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Codify.Windows.Converters
{
    public class BooleanConverter : IValueConverter
    {
        private const string ReverseParameter = "reverse";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ToBoolean(value, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof (bool) && value is bool)
            {
                var result = (bool) value;

                if (parameter is string && string.Equals((string) parameter, ReverseParameter, StringComparison.CurrentCultureIgnoreCase))
                {
                    result = !result;
                }

                return result;
            }

            throw new NotSupportedException();
        }

        public static bool ToBoolean(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result;

            if (value is string)
            {
                result = Convert((string) value);
            }
            else if (value is Visibility)
            {
                result = Convert((Visibility) value);
            }
            else if (value is bool)
            {
                result = (bool) value;
            }
            else
            {
                result = Convert(value);
            }

            if (parameter is string && string.Equals((string) parameter, ReverseParameter, StringComparison.CurrentCultureIgnoreCase))
            {
                result = !result;
            }

            return result;
        }

        private static bool Convert(string value)
        {
            bool result;
            if (bool.TryParse(value, out result))
            {
                return result;
            }
            return !string.IsNullOrEmpty(value);
        }

        private static bool Convert(Visibility value)
        {
            return value != Visibility.Collapsed;
        }

        private static bool Convert<T>(T value)
        {
            return !Equals(value, default(T));
        }
    }
}