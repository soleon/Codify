namespace Codify.System.Extensions;

/// <summary>
///     Contains extensions for enumerable types.
/// </summary>
public static class EnumerationExtensions
{
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
    ///     This method requires that <paramref name="list" /> is already sorted for it to function properly.
    /// </remarks>
    public static int ConditionalIndex<T>(
        this IList<T> list,
        T item,
        Func<T, T, int> comparer)
    {
        ArgumentNullException.ThrowIfNull(list);
        ArgumentNullException.ThrowIfNull(comparer);

        int startIndex = 0;
        int endIndex = list.Count - 1;
        while (startIndex <= endIndex)
        {
            int splitIndex = startIndex + (endIndex - startIndex) / 2;
            int result = comparer(list[splitIndex], item);
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
    public static void ConditionalInsert<T>(
        this IList<T> list,
        T item,
        Func<T, T, int> comparer)
    {
        int index = list.ConditionalIndex(item, comparer);
        if (index >= list.Count)
        {
            list.Add(item);
        }
        else
        {
            list.Insert(index, item);
        }
    }
}
