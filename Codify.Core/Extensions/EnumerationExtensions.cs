using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Codify.Extensions
{
    /// <summary>
    ///     Contains extensions for enumerable objects.
    /// </summary>
    public static class EnumerationExtensions
    {
        /// <summary>
        ///     Determines whether the specified enumerable is null or empty.
        /// </summary>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            return enumerable == null || !enumerable.Any();
        }

        /// <summary>
        ///     Performs the specified action on each element of the <see cref="enumerable" />.
        /// </summary>
        /// <typeparam name="T">Type of the elements in the enumerable.</typeparam>
        /// <param name="enumerable">The enuerable to perform the action on.</param>
        /// <param name="actions">
        ///     The list of <see cref="Action{T}" /> delegates to perform on each element of the <see cref="enumerable" />.
        /// </param>
        public static void ForEach<T>(this IEnumerable<T> enumerable, params Action<T>[] actions)
        {
            foreach (var i in enumerable)
                foreach (var a in actions)
                    a.ExecuteIfNotNull(i);
        }

        /// <summary>
        ///     Adds multiple items to a collection.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the collection.</typeparam>
        /// <param name="collection">The collection to add the items.</param>
        /// <param name="items">The items to be added.</param>
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            collection.UseIfNotNull(c => items.UseIfNotNull(itms => itms.ForEach(c.Add)));
        }

        /// <summary>
        ///     Adds multiple key value pairs to a dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dictionary">The dictionary to add the key value pairs.</param>
        /// <param name="items">The key value pairs to be added.</param>
        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            dictionary.UseIfNotNull(d => items.UseIfNotNull(itms => itms.ForEach(d.Add)));
        }

        /// <summary>
        ///     Removes multiple items to a collection.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the collection.</typeparam>
        /// <param name="collection">The collection to remove the items.</param>
        /// <param name="items">The items to be removed.</param>
        public static void RemoveRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            collection.UseIfNotNull(c => items.UseIfNotNull(itms => itms.ForEach(i => c.Remove(i))));
        }

        /// <summary>
        ///     Inserts an itme in a list according to the specified compare function.
        /// </summary>
        /// <typeparam name="T"> The type of the items in the list. </typeparam>
        /// <param name="list"> The list to insert the item. </param>
        /// <param name="item"> The item to be inserted. </param>
        /// <param name="comparer"> The compare function that compares 2 items of type T. First parameter: an item in the list. Second parameter: the item to be inserted. Retruns: A 32-bit signed integer indicating the relationship between the two comparands. Less than zero if first param is less than second param. Zero if first param equals second param. Greater than zero if first param is greater than second param. </param>
        public static void ConditionalInsert<T>(this IList<T> list, T item, Func<T, T, int> comparer)
        {
            var index = list.ConditionalIndex(item, comparer);
            if (index >= list.Count) list.Add(item);
            else list.Insert(index, item);
        }

        /// <summary>
        ///     Determines the index of the specified item in a list according to the given compare function.
        /// </summary>
        /// <typeparam name="T"> The type of the items in the list. </typeparam>
        /// <param name="list"> The list to insert the item. </param>
        /// <param name="item"> The item to be inserted. </param>
        /// <param name="comparer"> The compare function that compares 2 items of type T. First parameter: an item in the list. Second parameter: the item to be inserted. Retruns: A 32-bit signed integer indicating the relationship between the two comparands. Less than zero if first param is less than second param. Zero if first param equals second param. Greater than zero if first param is greater than second param. </param>
        /// <returns> The determined index of the specified item in a list according to the given compare function. </returns>
        public static int ConditionalIndex<T>(this IList<T> list, T item, Func<T, T, int> comparer)
        {
            var length = list.Count;
            for (var i = 0; i < length; i++)
            {
                if (comparer(list[i], item) <= 0) continue;
                return i;
            }
            return length;
        }

        /// <summary>
        ///     Appends the item if index is larger than the last item in the list, ignores the item if index is less than 0, otherwise, inserts the item at the index.
        /// </summary>
        /// <param name="list">The list to insert the item.</param>
        /// <param name="index">The intended index to insert the item.</param>
        /// <param name="item">The item to be inserted.</param>
        public static void SafeInsert(this IList list, int index, object item)
        {
            if (index < 0) return;
            if (index < list.Count) list.Insert(index, item);
            else list.Add(item);
        }

        public static void SafeRemoveAt(this IList list, int index)
        {
            if (index < 0 || index >= list.Count) return;
            list.RemoveAt(index);
        }

    }
}