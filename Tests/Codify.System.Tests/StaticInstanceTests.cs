using Codify.System;

namespace Codify.System.Tests;

public class StaticInstanceTests
{
    [Fact]
    public void InstanceReturnsSameObjectForClosedGenericType()
    {
        var first = StaticInstance<ReusableInstance>.Instance;
        var second = StaticInstance<ReusableInstance>.Instance;

        Assert.Same(first, second);
        Assert.Equal(1, ReusableInstance.CreatedCount);
    }

    [Fact]
    public void EachClosedGenericTypeMaintainsSeparateInstance()
    {
        var first = StaticInstance<FirstInstance>.Instance;
        var second = StaticInstance<SecondInstance>.Instance;

        Assert.NotSame(first, second);
    }

    private sealed class ReusableInstance
    {
        public static int CreatedCount { get; private set; }

        public ReusableInstance()
        {
            CreatedCount++;
        }
    }

    private sealed class FirstInstance;

    private sealed class SecondInstance;
}
