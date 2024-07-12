namespace Codify.System.Extensions;

/// <summary>
///     Contains extensions for enumerable types.
/// </summary>
public static class EnumerationExtensions
{
    /// <summary>
    ///     Uses the input collection to return a collection of batches based on the specified size.
    /// </summary>
    /// <typeparam name="T">The type of the enumerable item.</typeparam>
    /// <param name="source">The enumerable collection to batch from.</param>
    /// <param name="batchSize">The size of the batch operation.</param>
    public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
    {
        T[] batch = null;
        var count = 0;

        foreach (var item in source)
        {
            if (batch == null) batch = new T[batchSize];

            batch[count++] = item;

            if (count != batchSize) continue;

            yield return batch;

            batch = null;
            count = 0;
        }

        if (batch != null && count > 0) yield return batch.Take(count);
    }

    /// <summary>
    ///     Inserts an item in a list according to the specified compare function.
    /// </summary>
    /// <typeparam name="T">The type of the items in the list.</typeparam>
    /// <param name="list">The list to insert the item.</param>
    /// <param name="item">The item to be inserted.</param>
    /// <param name="comparer">
    ///     The compare function that compares 2 items of type T. First parameter: an item in the list.
    ///     Second parameter: the item to be inserted. Returns: A 32-bit signed integer indicating the relationship between the
    ///     two comparands. Less than zero if first param is less than second param. Zero if first param equals second param.
    ///     Greater than zero if first param is greater than second param.
    /// </param>
    public static void ConditionalInsert<T>(this IList<T> list, T item, Func<T, T, int> comparer)
    {
        var index = list.ConditionalIndex(item, comparer);
        if (index >= list.Count) list.Add(item);
        else list.Insert(index, item);
    }

    /// <summary>
    ///     Determines the index of the specified item in an already sorted list according to the given compare function.
    /// </summary>
    /// <typeparam name="T">The type of the items in the list.</typeparam>
    /// <param name="list">The list to insert the item.</param>
    /// <param name="item">The item to be inserted.</param>
    /// <param name="comparer">
    ///     The compare function that compares 2 items of type T. First parameter: an item in the list.
    ///     Second parameter: the item to be inserted. Returns: A 32-bit signed integer indicating the relationship between the
    ///     two comparands. Less than zero if first param is less than second param. Zero if first param equals second param.
    ///     Greater than zero if first param is greater than second param.
    /// </param>
    /// <returns>The determined index of the specified item in a list according to the given compare function.</returns>
    /// <remarks>
    ///     This method requires that the <see param="list" /> is already sorted for it to function properly.
    /// </remarks>
    public static int ConditionalIndex<T>(this IList<T> list, T item, Func<T, T, int> comparer)
    {
        var length = list.Count;
        if (length == 0)
            return 0;

        // The golden ratio is used to find the optimal splitting point.
        // This idea is known as the golden section search.
        // http://en.wikipedia.org/wiki/Golden_section_search
        const double goldenRatio = 1.61803398874989484820458683436;

        var startIndex = 0;
        var endIndex = length - 1;
        while (true)
        {
            // If the start index is larger than the end index,
            // that means the search has already passed all necessary
            // search ranges, just return the start index.
            if (startIndex > endIndex)
                return startIndex;

            // If the start index is the same as the end index,
            // that means there is only 1 item left to compare.
            // Compare this item and return the result.
            if (startIndex == endIndex)
                // If the item in the list is larger than the given item,
                // return the index of the item in the list, otherwise return the next index.
                return comparer(list[startIndex], item) > 0 ? startIndex : startIndex + 1;

            // If there are still multiple items in the search range,
            // find the optimal split index of the search range.
            var splitIndex = (int)(endIndex - (endIndex - startIndex) / goldenRatio);

            // Compare the item sitting at the splitting point.
            var result = comparer(list[splitIndex], item);
            if (result > 0)
                // If this item is larger than the given item,
                // keep searching the range before the splitting point.
                endIndex = splitIndex - 1;
            else if (result < 0)
                // If this item is smaller than the given item,
                // keep searching the range after it.
                startIndex = splitIndex + 1;
            else
                // If this item equals to the given item,
                // then we have found the index.
                return splitIndex;
        }
    }
}