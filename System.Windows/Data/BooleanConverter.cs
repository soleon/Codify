namespace Codify.System.Windows.Data;

/// <summary>
/// Converts values to Boolean binding values.
/// </summary>
public sealed class BooleanConverter : StaticInstance<BooleanConverter>, global::System.Windows.Data.IValueConverter
{
    internal static readonly object BoxedTrue = true;

    internal static readonly object BoxedFalse = false;

    private static readonly global::System.Collections.Concurrent.ConcurrentDictionary<global::System.Type, object>
        DefaultStructValues = new();

    /// <inheritdoc />
    public object Convert(
        object? value,
        global::System.Type targetType,
        object? parameter,
        global::System.Globalization.CultureInfo culture)
    {
        return Convert(value, parameter);
    }

    /// <summary>
    /// Converts the supplied value to a Boolean value without binding context.
    /// </summary>
    /// <param name="value">The value produced by the binding source.</param>
    /// <param name="parameter">Use the string value <c>invert</c> to invert the conversion result.</param>
    /// <returns>
    /// <see cref="global::System.Windows.DependencyProperty.UnsetValue" /> when <paramref name="value" /> is unset;
    /// <see cref="global::System.Windows.Data.Binding.DoNothing" /> when <paramref name="value" /> is do-nothing;
    /// otherwise, a shared cached <see cref="bool" /> instance to avoid per-binding allocations.
    /// </returns>
    [global::System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1822:Mark members as static",
        Justification = "Kept as instance so callers reach it through the StaticInstance singleton, mirroring the IValueConverter implementation.")]
    public object Convert(object? value, object? parameter = null)
    {
        if (value == global::System.Windows.DependencyProperty.UnsetValue ||
            value == global::System.Windows.Data.Binding.DoNothing)
        {
            return value;
        }

        var isInvert = parameter is string stringValue &&
                       stringValue.Equals("invert", global::System.StringComparison.OrdinalIgnoreCase);
        return value switch
        {
            bool boolValue => Box(boolValue ^ isInvert),
            null => Box(isInvert),
            _ => Box(IsDefaultValue(value) ? isInvert : !isInvert)
        };
    }

    internal static object Box(bool value)
    {
        return value ? BoxedTrue : BoxedFalse;
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
            global::System.IntPtr typedValue => typedValue == default,
            global::System.UIntPtr typedValue => typedValue == default,
            global::System.DateTime typedValue => typedValue == default,
            global::System.DateTimeOffset typedValue => typedValue == default,
            global::System.TimeSpan typedValue => typedValue == default,
            global::System.DateOnly typedValue => typedValue == default,
            global::System.TimeOnly typedValue => typedValue == default,
            global::System.Guid typedValue => typedValue == default,
            _ => IsDefaultStructValue(value)
        };
    }

    private static bool IsDefaultStructValue(object value)
    {
        var type = value.GetType();
        return type.IsValueType &&
               Equals(DefaultStructValues.GetOrAdd(
                   type,
                   static defaultType =>
                       global::System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(defaultType)),
                   value);
    }
}
