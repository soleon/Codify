using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Codify.System.Windows.Data;

public class MultiBooleanConverter : StaticInstance<MultiBooleanConverter>, IMultiValueConverter
{
    private static readonly BooleanConverter BooleanConverter = StaticInstance<BooleanConverter>.Instance;

    public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        foreach (var value in values)
        {
            var result = BooleanConverter.Convert(value);
            if (result == DependencyProperty.UnsetValue)
            {
                return DependencyProperty.UnsetValue;
            }

            if (!Equals(result, true))
            {
                return parameter ?? false;
            }
        }

        return true;
    }

    public object[]? ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        return null;
    }
}
