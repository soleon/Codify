using System.Globalization;
using System.Windows;
using Codify.System.Windows.Data;

namespace Codify.System.Windows.Tests.Data;

public class EqualityConverterTests
{
    [Fact]
    public void ConvertReturnsWhetherValueEqualsParameter()
    {
        Assert.True((bool)EqualityConverter.Instance.Convert("same", null!, "same", CultureInfo.InvariantCulture));
        Assert.False((bool)EqualityConverter.Instance.Convert("left", null!, "right", CultureInfo.InvariantCulture));
        Assert.True((bool)EqualityConverter.Instance.Convert(null!, null!, null!, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void ConvertBackReturnsParameterWhenValueIsTrue()
    {
        var parameter = new object();

        var result = EqualityConverter.Instance.ConvertBack(true, null!, parameter, CultureInfo.InvariantCulture);

        Assert.Same(parameter, result);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(null)]
    public void ConvertBackReturnsUnsetValueWhenValueIsNotTrue(object? value)
    {
        var result = EqualityConverter.Instance.ConvertBack(value!, null!, "parameter", CultureInfo.InvariantCulture);

        Assert.Same(DependencyProperty.UnsetValue, result);
    }

    [Fact]
    public void ConvertBackReturnsUnsetValueWhenValueIsNonBooleanTrue()
    {
        var result = EqualityConverter.Instance.ConvertBack("true", null!, "parameter", CultureInfo.InvariantCulture);

        Assert.Same(DependencyProperty.UnsetValue, result);
    }
}
