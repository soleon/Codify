using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Codify.System.Windows.Data;

/// <summary>
/// Converts values to Boolean binding values by comparing them with the converter parameter.
/// </summary>
public class EqualityConverter : StaticInstance<EqualityConverter>, IValueConverter
{
    /// <summary>
    /// Determines whether the supplied value equals the converter parameter.
    /// </summary>
    /// <param name="value">The value produced by the binding source.</param>
    /// <param name="targetType">The binding target type.</param>
    /// <param name="parameter">The value to compare with <paramref name="value" />.</param>
    /// <param name="culture">The culture to use for conversion.</param>
    /// <returns><see langword="true" /> when the values are equal; otherwise, <see langword="false" />.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Equals(value, parameter);
    }

    /// <summary>
    /// Converts a Boolean equality result back to the converter parameter.
    /// </summary>
    /// <param name="value">The Boolean value produced by the binding target.</param>
    /// <param name="targetType">The binding source type.</param>
    /// <param name="parameter">The value to return when <paramref name="value" /> is <see langword="true" />.</param>
    /// <param name="culture">The culture to use for conversion.</param>
    /// <returns>
    /// <paramref name="parameter" /> when <paramref name="value" /> is <see langword="true" />; otherwise,
    /// <see cref="DependencyProperty.UnsetValue" />.
    /// </returns>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true
            ? parameter
            : DependencyProperty.UnsetValue;
    }
}
