using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Codify.System.Windows.Data;

namespace Codify.System.Windows.Tests.Data;

public class BooleanConverterTests
{
    [Theory]
    [InlineData(true, null, true)]
    [InlineData(false, null, false)]
    [InlineData(true, "invert", false)]
    [InlineData(false, "invert", true)]
    [InlineData(null, null, false)]
    [InlineData(null, "invert", true)]
    public void ConvertHandlesBooleanAndNullValues(object? value, string? parameter, bool expected)
    {
        var result = BooleanConverter.Instance.Convert(value!, null!, parameter!, CultureInfo.InvariantCulture);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ConvertTreatsDefaultValueTypesAsFalseAndNonDefaultValuesAsTrue()
    {
        Assert.False((bool)BooleanConverter.Instance.Convert(0, null!, null!, CultureInfo.InvariantCulture));
        Assert.True((bool)BooleanConverter.Instance.Convert(7, null!, null!, CultureInfo.InvariantCulture));
        Assert.False((bool)BooleanConverter.Instance.Convert(DateTime.MinValue, null!, null!, CultureInfo.InvariantCulture));
        Assert.True((bool)BooleanConverter.Instance.Convert(new DateTime(2026, 4, 26), null!, null!, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void ConvertUsesCaseInsensitiveInvertParameterForValueTypes()
    {
        Assert.True((bool)BooleanConverter.Instance.Convert(0, null!, "INVERT", CultureInfo.InvariantCulture));
        Assert.False((bool)BooleanConverter.Instance.Convert(7, null!, "INVERT", CultureInfo.InvariantCulture));
    }

    [Fact]
    public void ConvertTreatsReferenceValuesAsTrue()
    {
        var value = new object();

        var result = BooleanConverter.Instance.Convert(value, null!, null!, CultureInfo.InvariantCulture);

        Assert.True((bool)result);
    }

    [Fact]
    public void ConvertReturnsUnsetValueUnchanged()
    {
        var result = BooleanConverter.Instance.Convert(
            DependencyProperty.UnsetValue,
            null!,
            "invert",
            CultureInfo.InvariantCulture);

        Assert.Same(DependencyProperty.UnsetValue, result);
    }

    [Fact]
    public void ConvertReturnsDoNothingUnchanged()
    {
        var result = BooleanConverter.Instance.Convert(
            Binding.DoNothing,
            null!,
            "invert",
            CultureInfo.InvariantCulture);

        Assert.Same(Binding.DoNothing, result);
    }

    [Fact]
    public void ConvertBackReturnsDoNothing()
    {
        var result = BooleanConverter.Instance.ConvertBack(true, null!, null!, CultureInfo.InvariantCulture);

        Assert.Same(Binding.DoNothing, result);
    }

    [Fact]
    public void ConvertWithoutContextDelegatesToInterfaceImplementation()
    {
        Assert.Equal(true, BooleanConverter.Instance.Convert(true));
        Assert.Equal(false, BooleanConverter.Instance.Convert(false));
        Assert.Equal(true, BooleanConverter.Instance.Convert(false, parameter: "invert"));
    }

    [Fact]
    public void ConverterImplementationCachesFallbackStructDefaults()
    {
        var source = global::System.IO.File.ReadAllText(FindBooleanConverterSourcePath());

        Assert.Contains("ConcurrentDictionary", source, global::System.StringComparison.Ordinal);
        Assert.Contains("GetOrAdd", source, global::System.StringComparison.Ordinal);
    }

    private static string FindBooleanConverterSourcePath()
    {
        var directory = new global::System.IO.DirectoryInfo(global::System.AppContext.BaseDirectory);
        while (directory != null)
        {
            var candidate = global::System.IO.Path.Combine(
                directory.FullName,
                "System.Windows",
                "Data",
                "BooleanConverter.cs");
            if (global::System.IO.File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new global::System.InvalidOperationException("Could not locate BooleanConverter.cs.");
    }
}
