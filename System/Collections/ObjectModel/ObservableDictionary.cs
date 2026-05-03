using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Codify.System.Collections.ObjectModel;

/// <summary>
///     Represents an observable collection whose items are also addressable by keys derived from each value.
/// </summary>
/// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of values in the collection.</typeparam>
public class ObservableDictionary<TKey, TValue> :
    DeferrableObservableCollection<TValue>,
    IDictionary<TKey, TValue>
    where TKey : notnull
{
    private static readonly bool IsValueType = typeof(TValue).IsValueType;

    private static readonly EqualityComparer<TKey> KeyComparer =
        EqualityComparer<TKey>.Default;

    private static readonly EqualityComparer<TValue> ValueComparer =
        EqualityComparer<TValue>.Default;

    private readonly Dictionary<TKey, TValue> _dictionary = new();
    private readonly Func<TValue, TKey> _getKey;
    private readonly Lock _syncRoot = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="ObservableDictionary{TKey,TValue}" /> class.
    /// </summary>
    /// <param name="keyProvider">The function that returns the dictionary key for a value.</param>
    public ObservableDictionary(Func<TValue, TKey> keyProvider)
    {
        ArgumentNullException.ThrowIfNull(keyProvider);

        _getKey = keyProvider;
    }

    void ICollection<KeyValuePair<TKey, TValue>>.
        Add(KeyValuePair<TKey, TValue> item)
    {
        lock (_syncRoot)
        {
            Add(item.Key, item.Value);
        }
    }

    /// <summary>
    ///     Adds a value with the specified key.
    /// </summary>
    /// <param name="key">The key for the value.</param>
    /// <param name="value">The value to add.</param>
    /// <exception cref="ArgumentException">
    ///     <paramref name="key" /> does not match the key produced from <paramref name="value" />.
    /// </exception>
    public void Add(TKey key, TValue value)
    {
        lock (_syncRoot)
        {
            TKey itemKey = GetValidatedItemKey(key, value);
            InsertItem(Count, itemKey, value);
        }
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.
        Contains(KeyValuePair<TKey, TValue> item)
    {
        lock (_syncRoot)
        {
            return Contains(item);
        }
    }

    /// <summary>
    ///     Determines whether the dictionary contains the specified key.
    /// </summary>
    /// <param name="key">The key to locate.</param>
    /// <returns><see langword="true" /> if the key exists; otherwise, <see langword="false" />.</returns>
    public bool ContainsKey(TKey key)
    {
        lock (_syncRoot)
        {
            return _dictionary.ContainsKey(key);
        }
    }

    void ICollection<KeyValuePair<TKey, TValue>>.
        CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        lock (_syncRoot)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)
                _dictionary).CopyTo(array, arrayIndex);
        }
    }

    IEnumerator<KeyValuePair<TKey, TValue>>
        IEnumerable<KeyValuePair<TKey, TValue>>.
        GetEnumerator()
    {
        return _dictionary.GetEnumerator();
    }

    /// <summary>
    ///     Gets a value indicating whether the dictionary is read-only. Always returns <see langword="false" />.
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    ///     Gets or sets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get or set.</param>
    /// <returns>The value associated with <paramref name="key" />.</returns>
    /// <exception cref="ArgumentException">
    ///     The assigned value produces a different key, or changing an item key would duplicate an existing key.
    /// </exception>
    public TValue this[TKey key]
    {
        get
        {
            lock (_syncRoot)
            {
                return _dictionary[key];
            }
        }
        set
        {
            lock (_syncRoot)
            {
                TKey itemKey = GetValidatedItemKey(key, value);
                if (!_dictionary.TryGetValue(key, out TValue? item))
                {
                    InsertItem(Count, itemKey, value);
                    return;
                }

                int index = IndexOf(key, item);
                if (index < 0)
                {
                    Debug.Fail(
                        "ObservableDictionary lost the base list entry for a stored key. The dictionary entry is being rebuilt at the end of the list to recover.");
                    _dictionary.Remove(key);
                    InsertItem(Count, itemKey, value);
                    return;
                }

                SetItem(index, key, itemKey, value);
            }
        }
    }

    /// <summary>
    ///     Gets a snapshot of the dictionary keys taken under the synchronisation lock.
    /// </summary>
    /// <remarks>
    ///     Each access allocates a new list copy of the current keys so that callers can iterate safely while the
    ///     underlying collection mutates. Avoid calling this property on hot paths.
    /// </remarks>
    public ICollection<TKey> Keys
    {
        get
        {
            lock (_syncRoot)
            {
                return [.. _dictionary.Keys];
            }
        }
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.
        Remove(KeyValuePair<TKey, TValue> item)
    {
        lock (_syncRoot)
        {
            return Contains(item) && Remove(item.Key);
        }
    }

    /// <summary>
    ///     Removes the value with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to remove.</param>
    /// <returns><see langword="true" /> if a value was removed; otherwise, <see langword="false" />.</returns>
    public bool Remove(TKey key)
    {
        lock (_syncRoot)
        {
            if (!_dictionary.TryGetValue(key, out TValue? item))
            {
                return false;
            }

            int index = IndexOf(key, item);
            if (index < 0)
            {
                Debug.Fail(
                    "ObservableDictionary lost the base list entry for a stored key. The dictionary entry is being removed to recover.");
                _dictionary.Remove(key);
                return true;
            }

            RemoveItem(index);
            return true;
        }
    }

    /// <summary>
    ///     Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <param name="value">
    ///     When this method returns, contains the value associated with the specified key when found; otherwise, the
    ///     default value for <typeparamref name="TValue" />.
    /// </param>
    /// <returns><see langword="true" /> if the key was found; otherwise, <see langword="false" />.</returns>
    public bool TryGetValue(
        TKey key,
        [MaybeNullWhen(false)] out TValue value)
    {
        lock (_syncRoot)
        {
            return _dictionary.TryGetValue(key, out value);
        }
    }

    /// <summary>
    ///     Gets a snapshot of the dictionary values taken under the synchronisation lock.
    /// </summary>
    /// <remarks>
    ///     Each access allocates a new list copy of the current values so that callers can iterate safely while
    ///     the underlying collection mutates. Avoid calling this property on hot paths.
    /// </remarks>
    public ICollection<TValue> Values
    {
        get
        {
            lock (_syncRoot)
            {
                return [.. _dictionary.Values];
            }
        }
    }

    private static bool IsSameStoredItem(TValue storedItem, TValue item)
    {
        return ReferenceEquals(storedItem, item) ||
               (typeof(TValue).IsValueType && ValueComparer.Equals(storedItem, item));
    }

    /// <summary>
    ///     Removes all items from the collection and dictionary.
    /// </summary>
    protected override void ClearItems()
    {
        lock (_syncRoot)
        {
            CheckReentrancy();
            _dictionary.Clear();
            base.ClearItems();
        }
    }

    private bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return _dictionary.TryGetValue(item.Key, out TValue? value) &&
               ValueComparer.Equals(value, item.Value);
    }

    private TKey GetStoredItemKey(TValue item)
    {
        TKey itemKey = _getKey(item);
        if (_dictionary.TryGetValue(itemKey, out TValue? value) && IsSameStoredItem(value, item))
        {
            return itemKey;
        }

        foreach (KeyValuePair<TKey, TValue> pair in _dictionary)
        {
            if (IsSameStoredItem(pair.Value, item))
            {
                return pair.Key;
            }
        }

        return itemKey;
    }

    private TKey GetValidatedItemKey(TKey key, TValue value)
    {
        TKey itemKey = _getKey(value);
        if (!KeyComparer.Equals(key, itemKey))
        {
            throw new ArgumentException("The key must match the key provided by the value.", nameof(key));
        }

        return itemKey;
    }

    private int IndexOf(TKey key, TValue value)
    {
        for (int index = 0; index < Items.Count; index++)
        {
            TValue item = Items[index];
            if (ReferenceEquals(item, value))
            {
                return index;
            }

            if (IsValueType && KeyComparer.Equals(_getKey(item), key) && ValueComparer.Equals(item, value))
            {
                return index;
            }
        }

        return -1;
    }

    /// <summary>
    ///     Inserts an item into the collection and dictionary at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which to insert the item.</param>
    /// <param name="item">The item to insert.</param>
    protected override void InsertItem(int index, TValue item)
    {
        lock (_syncRoot)
        {
            InsertItem(index, _getKey(item), item);
        }
    }

    private void InsertItem(int index, TKey key, TValue item)
    {
        CheckReentrancy();
        if ((uint)index > (uint)Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        _dictionary.Add(key, item);
        base.InsertItem(index, item);
    }

    private void RemoveDictionaryEntry(TValue item)
    {
        TKey key = _getKey(item);
        if (_dictionary.TryGetValue(key, out TValue? value) && IsSameStoredItem(value, item))
        {
            _dictionary.Remove(key);
            return;
        }

        TKey? keyToRemove = default;
        bool shouldRemove = false;
        foreach (KeyValuePair<TKey, TValue> pair in _dictionary)
        {
            if (IsSameStoredItem(pair.Value, item))
            {
                keyToRemove = pair.Key;
                shouldRemove = true;
                break;
            }
        }

        if (shouldRemove)
        {
            _dictionary.Remove(keyToRemove!);
        }
    }

    /// <summary>
    ///     Removes the item at the specified index from the collection and dictionary.
    /// </summary>
    /// <param name="index">The zero-based index of the item to remove.</param>
    protected override void RemoveItem(int index)
    {
        lock (_syncRoot)
        {
            CheckReentrancy();
            RemoveDictionaryEntry(Items[index]);
            base.RemoveItem(index);
        }
    }

    /// <summary>
    ///     Replaces the item at the specified index and keeps the dictionary key mapping synchronized.
    /// </summary>
    /// <param name="index">The zero-based index of the item to replace.</param>
    /// <param name="item">The replacement item.</param>
    protected override void SetItem(int index, TValue item)
    {
        lock (_syncRoot)
        {
            TValue oldItem = Items[index];
            SetItem(index, GetStoredItemKey(oldItem), _getKey(item), item);
        }
    }

    private void SetItem(int index, TKey oldKey, TKey newKey, TValue item)
    {
        CheckReentrancy();
        if (KeyComparer.Equals(oldKey, newKey))
        {
            _dictionary[oldKey] = item;
            base.SetItem(index, item);
            return;
        }

        if (_dictionary.ContainsKey(newKey))
        {
            throw new ArgumentException("An item with the same key already exists.", nameof(item));
        }

        _dictionary.Remove(oldKey);
        _dictionary.Add(newKey, item);
        base.SetItem(index, item);
    }
}
