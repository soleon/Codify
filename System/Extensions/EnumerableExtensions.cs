namespace Codify.System.Extensions;

/// <summary>
/// Contains extensions for enumerable types.
/// </summary>
public static class EnumerationExtensions
{
    /// <summary>
    /// Uses the input collection to return a collection of batches based on the specified size.
    /// </summary>
    /// <typeparam name="T">The type of the enumerable item.</typeparam>
    /// <param name="source">The enumerable collection to batch from.</param>
    /// <param name="batchSize">The size of the batch operation.</param>
    public static global::System.Collections.Generic.IEnumerable<global::System.Collections.Generic.IEnumerable<T>> Batch<T>(
        this global::System.Collections.Generic.IEnumerable<T> source,
        int batchSize)
    {
        return global::System.Linq.Enumerable.Chunk(source, batchSize);
    }

    /// <summary>
    /// Inserts an item in a list according to the specified compare function.
    /// </summary>
    /// <typeparam name="T">The type of the items in the list.</typeparam>
    /// <param name="list">The list to insert the item.</param>
    /// <param name="item">The item to be inserted.</param>
    /// <param name="comparer">
    /// The compare function that compares 2 items of type T. First parameter: an item in the list.
    /// Second parameter: the item to be inserted. Returns: A 32-bit signed integer indicating the relationship between the
    /// two comparands. Less than zero if first param is less than second param. Zero if first param equals second param.
    /// Greater than zero if first param is greater than second param.
    /// </param>
    public static void ConditionalInsert<T>(
        this global::System.Collections.Generic.IList<T> list,
        T item,
        global::System.Func<T, T, int> comparer)
    {
        var index = list.ConditionalIndex(item, comparer);
        if (index >= list.Count) list.Add(item);
        else list.Insert(index, item);
    }

    /// <summary>
    /// Determines the index of the specified item in an already sorted list according to the given compare function.
    /// </summary>
    /// <typeparam name="T">The type of the items in the list.</typeparam>
    /// <param name="list">The list to insert the item.</param>
    /// <param name="item">The item to be inserted.</param>
    /// <param name="comparer">
    /// The compare function that compares 2 items of type T. First parameter: an item in the list.
    /// Second parameter: the item to be inserted. Returns: A 32-bit signed integer indicating the relationship between the
    /// two comparands. Less than zero if first param is less than second param. Zero if first param equals second param.
    /// Greater than zero if first param is greater than second param.
    /// </param>
    /// <returns>The determined index of the specified item in a list according to the given compare function.</returns>
    /// <remarks>
    /// This method requires that <paramref name="list" /> is already sorted for it to function properly.
    /// </remarks>
    public static int ConditionalIndex<T>(
        this global::System.Collections.Generic.IList<T> list,
        T item,
        global::System.Func<T, T, int> comparer)
    {
        global::System.ArgumentNullException.ThrowIfNull(list);
        global::System.ArgumentNullException.ThrowIfNull(comparer);

        var startIndex = 0;
        var endIndex = list.Count - 1;
        while (startIndex <= endIndex)
        {
            var splitIndex = startIndex + (endIndex - startIndex) / 2;
            var result = comparer(list[splitIndex], item);
            if (result > 0)
            {
                endIndex = splitIndex - 1;
            }
            else if (result < 0)
            {
                startIndex = splitIndex + 1;
            }
            else
            {
                return splitIndex;
            }
        }

        return startIndex;
    }
}
