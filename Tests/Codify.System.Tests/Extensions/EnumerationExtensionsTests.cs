using Codify.System.Extensions;

namespace Codify.System.Tests.Extensions;

public class EnumerationExtensionsTests
{
#pragma warning disable CS0618 // Batch is intentionally obsolete; preserve regression coverage.

    [Fact]
    public void BatchSplitsSourceIntoFullAndPartialBatchesInOrder()
    {
        var batches = Enumerable.Range(1, 5)
            .Batch(2)
            .Select(batch => batch.ToArray())
            .ToArray();

        Assert.Collection(
            batches,
            batch => Assert.Equal([1, 2], batch),
            batch => Assert.Equal([3, 4], batch),
            batch => Assert.Equal([5], batch));
    }

    [Fact]
    public void BatchReturnsNoBatchesForEmptySource()
    {
        var batches = Array.Empty<int>().Batch(3);

        Assert.Empty(batches);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void BatchRejectsNonPositiveBatchSize(int batchSize)
    {
        var source = Enumerable.Range(1, 3);

        Assert.Throws<ArgumentOutOfRangeException>(() => source.Batch(batchSize).ToArray());
    }

#pragma warning restore CS0618

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
