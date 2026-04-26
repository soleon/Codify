using System.Globalization;
using System.Windows;
using Codify.System.Windows.Data;

namespace Codify.System.Windows.Tests.Data;

public class VisibilityConverterTests
{
    [Theory]
    [InlineData(true, null, Visibility.Visible)]
    [InlineData(false, null, Visibility.Collapsed)]
    [InlineData(true, "invert", Visibility.Collapsed)]
    [InlineData(false, "invert", Visibility.Visible)]
    public void ConvertMapsBooleanValuesToVisibility(object value, string? parameter, Visibility expected)
    {
        var result = VisibilityConverter.Instance.Convert(
            value,
            typeof(Visibility),
            parameter!,
            CultureInfo.InvariantCulture);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ConvertReturnsUnsetValueUnchanged()
    {
        var result = VisibilityConverter.Instance.Convert(
            DependencyProperty.UnsetValue,
            typeof(Visibility),
            null!,
            CultureInfo.InvariantCulture);

        Assert.Same(DependencyProperty.UnsetValue, result);
    }

    [Fact]
    public void ConvertBackIsNotImplemented()
    {
        Assert.Throws<NotImplementedException>(
            () => VisibilityConverter.Instance.ConvertBack(Visibility.Visible, typeof(bool), null!, CultureInfo.InvariantCulture));
    }
}
