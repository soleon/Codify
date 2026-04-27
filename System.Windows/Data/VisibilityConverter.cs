namespace Codify.System.Windows.Data;

/// <summary>
/// Converts values to WPF <see cref="global::System.Windows.Visibility" /> values.
/// </summary>
public sealed class VisibilityConverter : StaticInstance<VisibilityConverter>, global::System.Windows.Data.IValueConverter
{
    private static readonly object BoxedVisible = global::System.Windows.Visibility.Visible;

    private static readonly object BoxedCollapsed = global::System.Windows.Visibility.Collapsed;

    /// <summary>
    /// Converts the supplied value to <see cref="global::System.Windows.Visibility.Visible" /> or
    /// <see cref="global::System.Windows.Visibility.Collapsed" />.
    /// </summary>
    /// <param name="value">The value produced by the binding source.</param>
    /// <param name="targetType">The binding target type.</param>
    /// <param name="parameter">A parameter passed through to <see cref="BooleanConverter" />.</param>
    /// <param name="culture">The culture to use for conversion.</param>
    /// <returns>
    /// <see cref="global::System.Windows.DependencyProperty.UnsetValue" /> when <paramref name="value" /> is unset;
    /// otherwise, a shared cached <see cref="global::System.Windows.Visibility" /> instance to avoid
    /// per-binding allocations.
    /// </returns>
    public object Convert(
        object? value,
        global::System.Type targetType,
        object? parameter,
        global::System.Globalization.CultureInfo culture)
    {
        var result = BooleanConverter.Instance.Convert(value, targetType, parameter, culture);
        return result == global::System.Windows.DependencyProperty.UnsetValue ||
               result == global::System.Windows.Data.Binding.DoNothing
            ? result
            : result is true ? BoxedVisible : BoxedCollapsed;
    }

    /// <summary>
    /// Converts a binding target value back to the source value.
    /// </summary>
    /// <param name="value">The value produced by the binding target.</param>
    /// <param name="targetType">The binding source type.</param>
    /// <param name="parameter">An optional converter parameter.</param>
    /// <param name="culture">The culture to use for conversion.</param>
    /// <returns>
    /// <see cref="global::System.Windows.Data.Binding.DoNothing" /> because reverse conversion is not supported.
    /// </returns>
    public object ConvertBack(
        object? value,
        global::System.Type targetType,
        object? parameter,
        global::System.Globalization.CultureInfo culture)
    {
        return global::System.Windows.Data.Binding.DoNothing;
    }
}
