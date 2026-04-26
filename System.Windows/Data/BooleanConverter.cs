using System.Globalization;
using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Data;

namespace Codify.System.Windows.Data;

/// <summary>
/// Converts values to Boolean binding values.
/// </summary>
public class BooleanConverter : StaticInstance<BooleanConverter>, IValueConverter
{
    private static readonly ConcurrentDictionary<Type, object?> DefaultValues = new();

    /// <summary>
    /// Converts the supplied value to a Boolean value for a binding target.
    /// </summary>
    /// <param name="value">The value produced by the binding source.</param>
    /// <param name="targetType">The binding target type.</param>
    /// <param name="parameter">Use the string value <c>invert</c> to invert the conversion result.</param>
    /// <param name="culture">The culture to use for conversion.</param>
    /// <returns>
    /// <see cref="DependencyProperty.UnsetValue" /> when <paramref name="value" /> is unset; otherwise, the converted
    /// Boolean value.
    /// </returns>
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

    /// <summary>
    /// Converts a binding target value back to the source value.
    /// </summary>
    /// <param name="value">The value produced by the binding target.</param>
    /// <param name="targetType">The binding source type.</param>
    /// <param name="parameter">An optional converter parameter.</param>
    /// <param name="culture">The culture to use for conversion.</param>
    /// <returns>This method does not return a value.</returns>
    /// <exception cref="NotImplementedException">The conversion is not implemented.</exception>
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
