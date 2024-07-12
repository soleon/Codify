using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Codify.System.Windows.Data;

public class MultiBooleanConverter : StaticInstance<MultiBooleanConverter>, IMultiValueConverter
{
    private static readonly BooleanConverter BooleanConverter = StaticInstance<BooleanConverter>.Instance;

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var results = values.Select(v => BooleanConverter.Convert(v)).ToList();

        if (results.Contains(DependencyProperty.UnsetValue)) return DependencyProperty.UnsetValue;

        var result = results.All(x => Equals(x, true));
        return result ? true : parameter ?? false;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        return null;
    }
}