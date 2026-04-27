using Codify.System.Extensions;

namespace Codify.System.Tests.Extensions;

public class EnumerationExtensionsTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(2, 1)]
    [InlineData(4, 2)]
    [InlineData(6, 3)]
    public void ConditionalIndexReturnsInsertionIndexForSortedList(int item, int expectedIndex)
    {
        var list = new List<int> { 1, 3, 5 };

        var index = list.ConditionalIndex(item, Compare);

        Assert.Equal(expectedIndex, index);
    }

    [Fact]
    public void ConditionalInsertMaintainsSortedOrder()
    {
        var list = new List<int> { 1, 3, 5 };

        list.ConditionalInsert(4, Compare);
        list.ConditionalInsert(0, Compare);
        list.ConditionalInsert(6, Compare);

        Assert.Equal([0, 1, 3, 4, 5, 6], list);
    }

    private static int Compare(int left, int right)
    {
        return left.CompareTo(right);
    }
}
