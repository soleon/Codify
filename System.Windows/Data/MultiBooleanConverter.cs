using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Codify.System.Windows.Data;

/// <summary>
///     Converts multiple binding values to a single Boolean value.
/// </summary>
public sealed class MultiBooleanConverter : StaticInstance<MultiBooleanConverter>, IMultiValueConverter
{
    private static readonly BooleanConverter Default = StaticInstance<BooleanConverter>.Instance;

    /// <summary>
    ///     Converts all supplied values and returns <see langword="true" /> only when every value converts to true.
    /// </summary>
    /// <param name="values">
    ///     The values produced by the binding sources. Individual entries may be <see langword="null" />,
    ///     <see cref="global::System.Windows.DependencyProperty.UnsetValue" />, or
    ///     <see cref="global::System.Windows.Data.Binding.DoNothing" />.
    /// </param>
    /// <param name="targetType">The binding target type.</param>
    /// <param name="parameter">The value to return when any value converts to false.</param>
    /// <param name="culture">The culture to use for conversion.</param>
    /// <returns>
    ///     <see langword="true" /> when all values convert to true; <paramref name="parameter" /> or
    ///     <see langword="false" /> when any value converts to false; otherwise,
    ///     <see cref="global::System.Windows.DependencyProperty.UnsetValue" /> when any value is unset, or
    ///     <see cref="global::System.Windows.Data.Binding.DoNothing" /> when any value is do-nothing.
    /// </returns>
    public object Convert(
        object?[] values,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(values);

        foreach (object? value in values)
        {
            object result = Default.Convert(value, null);
            if (result == DependencyProperty.UnsetValue ||
                result == Binding.DoNothing)
            {
                return result;
            }

            if (result is not true)
            {
                return parameter ?? BooleanConverter.BoxedFalse;
            }
        }

        return BooleanConverter.BoxedTrue;
    }

    /// <summary>
    ///     Converts a binding target value back to the source values.
    /// </summary>
    /// <param name="value">The value produced by the binding target.</param>
    /// <param name="targetTypes">The binding source types.</param>
    /// <param name="parameter">An optional converter parameter.</param>
    /// <param name="culture">The culture to use for conversion.</param>
    /// <returns><see langword="null" /> because reverse conversion is not supported.</returns>
    public object?[]? ConvertBack(
        object? value,
        Type[] targetTypes,
        object? parameter,
        CultureInfo culture)
    {
        return null;
    }
}
