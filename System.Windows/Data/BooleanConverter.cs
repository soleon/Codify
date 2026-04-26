using System.Globalization;
using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Data;

namespace Codify.System.Windows.Data;

public class BooleanConverter : StaticInstance<BooleanConverter>, IValueConverter
{
    private static readonly ConcurrentDictionary<Type, object?> DefaultValues = new();

    public object Convert(object? value, Type? targetType = null, object? parameter = null, CultureInfo? culture = null)
    {
        if (value == DependencyProperty.UnsetValue) return value;

        var isInvert = parameter is string stringValue &&
                       stringValue.Equals("invert", StringComparison.OrdinalIgnoreCase);
        switch (value)
        {
            case bool boolValue:
                return isInvert ? !boolValue : boolValue;
            case null:
                return isInvert;
            default:
                return IsDefaultValue(value) ? isInvert : !isInvert;
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private static object? GetDefaultValue(Type type)
    {
        return DefaultValues.GetOrAdd(type, static valueType => Activator.CreateInstance(valueType));
    }

    private static bool IsDefaultValue(object value)
    {
        var type = value.GetType();
        return type.IsValueType && Equals(GetDefaultValue(type), value);
    }
}
