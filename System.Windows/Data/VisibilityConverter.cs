using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Codify.System.Windows.Data;

/// <summary>
/// Converts values to WPF <see cref="Visibility" /> values.
/// </summary>
public sealed class VisibilityConverter : StaticInstance<VisibilityConverter>, IValueConverter
{
    /// <summary>
    /// Converts the supplied value to <see cref="Visibility.Visible" /> or <see cref="Visibility.Collapsed" />.
    /// </summary>
    /// <param name="value">The value produced by the binding source.</param>
    /// <param name="targetType">The binding target type.</param>
    /// <param name="parameter">A parameter passed through to <see cref="BooleanConverter" />.</param>
    /// <param name="culture">The culture to use for conversion.</param>
    /// <returns>
    /// <see cref="DependencyProperty.UnsetValue" /> when <paramref name="value" /> is unset; otherwise,
    /// <see cref="Visibility.Visible" /> or <see cref="Visibility.Collapsed" />.
    /// </returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var result = BooleanConverter.Instance.Convert(value, targetType, parameter, culture);
        return result == DependencyProperty.UnsetValue ? result :
            result is true ? Visibility.Visible : Visibility.Collapsed;
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
}
