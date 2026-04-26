using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Codify.System.Windows.Data;

namespace Codify.System.Windows.Tests.Data;

public class MultiBooleanConverterTests
{
    [Fact]
    public void ConvertReturnsTrueWhenAllValuesConvertToTrue()
    {
        var result = MultiBooleanConverter.Instance.Convert(
            [true, 1, "text"],
            null!,
            null!,
            CultureInfo.InvariantCulture);

        Assert.True((bool)result);
    }

    [Fact]
    public void ConvertReturnsTrueForEmptyValueSet()
    {
        var result = MultiBooleanConverter.Instance.Convert(
            [],
            null!,
            "fallback",
            CultureInfo.InvariantCulture);

        Assert.True((bool)result);
    }

    [Fact]
    public void ConvertReturnsFalseParameterWhenAnyValueConvertsToFalse()
    {
        var result = MultiBooleanConverter.Instance.Convert(
            [true, false],
            null!,
            "fallback",
            CultureInfo.InvariantCulture);

        Assert.Equal("fallback", result);
    }

    [Fact]
    public void ConvertReturnsFalseWhenAnyValueConvertsToFalseAndNoParameterIsSupplied()
    {
        var result = MultiBooleanConverter.Instance.Convert(
            [true, 0],
            null!,
            null!,
            CultureInfo.InvariantCulture);

        Assert.False((bool)result);
    }

    [Fact]
    public void ConvertReturnsUnsetValueWhenAnyValueIsUnsetValue()
    {
        var result = MultiBooleanConverter.Instance.Convert(
            [true, DependencyProperty.UnsetValue],
            null!,
            null!,
            CultureInfo.InvariantCulture);

        Assert.Same(DependencyProperty.UnsetValue, result);
    }

    [Fact]
    public void ConvertReturnsDoNothingWhenAnyValueIsDoNothing()
    {
        var result = MultiBooleanConverter.Instance.Convert(
            [true, Binding.DoNothing],
            null!,
            "fallback",
            CultureInfo.InvariantCulture);

        Assert.Same(Binding.DoNothing, result);
    }

    [Fact]
    public void ConvertBackReturnsNull()
    {
        var result = MultiBooleanConverter.Instance.ConvertBack(
            true,
            [typeof(bool)],
            null!,
            CultureInfo.InvariantCulture);

        Assert.Null(result);
    }
}
