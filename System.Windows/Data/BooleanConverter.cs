using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;

namespace Codify.System.Windows.Data;

/// <summary>
///     Converts values to Boolean binding values.
/// </summary>
public sealed class BooleanConverter : StaticInstance<BooleanConverter>, IValueConverter
{
    internal static readonly object BoxedFalse = false;
    internal static readonly object BoxedTrue = true;

    private static readonly ConcurrentDictionary<Type, object>
        DefaultStructValues = new();

    /// <inheritdoc />
    public object Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        return Convert(value, parameter);
    }

    /// <summary>
    ///     Converts a binding target value back to the source value.
    /// </summary>
    /// <param name="value">The value produced by the binding target.</param>
    /// <param name="targetType">The binding source type.</param>
    /// <param name="parameter">An optional converter parameter.</param>
    /// <param name="culture">The culture to use for conversion.</param>
    /// <returns>
    ///     <see cref="global::System.Windows.Data.Binding.DoNothing" /> because reverse conversion is not supported.
    /// </returns>
    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        return Binding.DoNothing;
    }

    internal static object Box(bool value)
    {
        return value ? BoxedTrue : BoxedFalse;
    }

    private static bool IsDefaultStructValue(object value)
    {
        Type type = value.GetType();
        return type.IsValueType &&
               Equals(DefaultStructValues.GetOrAdd(
                       type,
                       static defaultType =>
                           RuntimeHelpers.GetUninitializedObject(defaultType)),
                   value);
    }

    private static bool IsDefaultValue(object value)
    {
        return value switch
        {
            char typedValue => typedValue == default,
            byte typedValue => typedValue == default,
            sbyte typedValue => typedValue == default,
            short typedValue => typedValue == default,
            ushort typedValue => typedValue == default,
            int typedValue => typedValue == default,
            uint typedValue => typedValue == default,
            long typedValue => typedValue == default,
            ulong typedValue => typedValue == default,
            float typedValue => typedValue == default,
            double typedValue => typedValue == default,
            decimal typedValue => typedValue == default,
            IntPtr typedValue => typedValue == default,
            UIntPtr typedValue => typedValue == default,
            DateTime typedValue => typedValue == default,
            DateTimeOffset typedValue => typedValue == default,
            TimeSpan typedValue => typedValue == default,
            DateOnly typedValue => typedValue == default,
            TimeOnly typedValue => typedValue == default,
            Guid typedValue => typedValue == default,
            _ => IsDefaultStructValue(value)
        };
    }

    /// <summary>
    ///     Converts the supplied value to a Boolean value without binding context.
    /// </summary>
    /// <param name="value">The value produced by the binding source.</param>
    /// <param name="parameter">Use the string value <c>invert</c> to invert the conversion result.</param>
    /// <returns>
    ///     <see cref="global::System.Windows.DependencyProperty.UnsetValue" /> when <paramref name="value" /> is unset;
    ///     <see cref="global::System.Windows.Data.Binding.DoNothing" /> when <paramref name="value" /> is do-nothing;
    ///     otherwise, a shared cached <see cref="bool" /> instance to avoid per-binding allocations.
    /// </returns>
    [SuppressMessage(
        "Performance",
        "CA1822:Mark members as static",
        Justification =
            "Kept as instance so callers reach it through the StaticInstance singleton, mirroring the IValueConverter implementation.")]
    public object Convert(object? value, object? parameter = null)
    {
        if (value == DependencyProperty.UnsetValue ||
            value == Binding.DoNothing)
        {
            return value;
        }

        bool isInvert = parameter is string stringValue &&
                        stringValue.Equals("invert", StringComparison.OrdinalIgnoreCase);
        return value switch
        {
            bool boolValue => Box(boolValue ^ isInvert),
            null => Box(isInvert),
            _ => Box(IsDefaultValue(value) ? isInvert : !isInvert)
        };
    }
}
