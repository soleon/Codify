namespace Codify.System.Collections.ObjectModel;

/// <summary>
/// Represents an observable collection whose items are also addressable by keys derived from each value.
/// </summary>
/// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of values in the collection.</typeparam>
public class ObservableDictionary<TKey, TValue> :
    DeferrableObservableCollection<TValue>,
    global::System.Collections.Generic.IDictionary<TKey, TValue>
    where TKey : notnull
{
    private static readonly global::System.Collections.Generic.EqualityComparer<TKey> KeyComparer =
        global::System.Collections.Generic.EqualityComparer<TKey>.Default;

    private static readonly global::System.Collections.Generic.EqualityComparer<TValue> ValueComparer =
        global::System.Collections.Generic.EqualityComparer<TValue>.Default;

    private readonly global::System.Collections.Generic.Dictionary<TKey, TValue> _dictionary = new();
    private readonly global::System.Func<TValue, TKey> _getKey;
    private readonly global::System.Threading.Lock _syncRoot = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableDictionary{TKey,TValue}" /> class.
    /// </summary>
    /// <param name="keyProvider">The function that returns the dictionary key for a value.</param>
    public ObservableDictionary(global::System.Func<TValue, TKey> keyProvider)
    {
        global::System.ArgumentNullException.ThrowIfNull(keyProvider);

        _getKey = keyProvider;
    }

    global::System.Collections.Generic.IEnumerator<global::System.Collections.Generic.KeyValuePair<TKey, TValue>>
        global::System.Collections.Generic.IEnumerable<global::System.Collections.Generic.KeyValuePair<TKey, TValue>>.
        GetEnumerator()
    {
        return _dictionary.GetEnumerator();
    }

    void global::System.Collections.Generic.ICollection<global::System.Collections.Generic.KeyValuePair<TKey, TValue>>.
        Add(global::System.Collections.Generic.KeyValuePair<TKey, TValue> item)
    {
        lock (_syncRoot)
        {
            Add(item.Key, item.Value);
        }
    }

    bool global::System.Collections.Generic.ICollection<global::System.Collections.Generic.KeyValuePair<TKey, TValue>>.
        Contains(global::System.Collections.Generic.KeyValuePair<TKey, TValue> item)
    {
        lock (_syncRoot)
        {
            return Contains(item);
        }
    }

    void global::System.Collections.Generic.ICollection<global::System.Collections.Generic.KeyValuePair<TKey, TValue>>.
        CopyTo(global::System.Collections.Generic.KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        lock (_syncRoot)
        {
            ((global::System.Collections.Generic.ICollection<global::System.Collections.Generic.KeyValuePair<TKey, TValue>>)
                _dictionary).CopyTo(array, arrayIndex);
        }
    }

    bool global::System.Collections.Generic.ICollection<global::System.Collections.Generic.KeyValuePair<TKey, TValue>>.
        Remove(global::System.Collections.Generic.KeyValuePair<TKey, TValue> item)
    {
        lock (_syncRoot)
        {
            return Contains(item) && Remove(item.Key);
        }
    }

    /// <summary>
    /// Gets a value indicating whether the dictionary is read-only.
    /// </summary>
    public bool IsReadOnly { get; }

    /// <summary>
    /// Adds a value with the specified key.
    /// </summary>
    /// <param name="key">The key for the value.</param>
    /// <param name="value">The value to add.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="key" /> does not match the key produced from <paramref name="value" />.
    /// </exception>
    public void Add(TKey key, TValue value)
    {
        lock (_syncRoot)
        {
            var itemKey = GetValidatedItemKey(key, value);
            InsertItem(Count, itemKey, value);
        }
    }

    /// <summary>
    /// Determines whether the dictionary contains the specified key.
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

    /// <summary>
    /// Removes the value with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to remove.</param>
    /// <returns><see langword="true" /> if a value was removed; otherwise, <see langword="false" />.</returns>
    public bool Remove(TKey key)
    {
        lock (_syncRoot)
        {
            if (!_dictionary.TryGetValue(key, out var item))
            {
                return false;
            }

            var index = IndexOf(key, item);
            if (index < 0)
            {
                _dictionary.Remove(key);
                return true;
            }

            RemoveItem(index);
            return true;
        }
    }

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <param name="value">
    /// When this method returns, contains the value associated with the specified key when found; otherwise, the
    /// default value for <typeparamref name="TValue" />.
    /// </param>
    /// <returns><see langword="true" /> if the key was found; otherwise, <see langword="false" />.</returns>
    public bool TryGetValue(
        TKey key,
        [global::System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out TValue value)
    {
        lock (_syncRoot)
        {
            return _dictionary.TryGetValue(key, out value);
        }
    }

    /// <summary>
    /// Gets or sets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get or set.</param>
    /// <returns>The value associated with <paramref name="key" />.</returns>
    /// <exception cref="ArgumentException">
    /// The assigned value produces a different key, or changing an item key would duplicate an existing key.
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
                var itemKey = GetValidatedItemKey(key, value);
                if (!_dictionary.TryGetValue(key, out var item))
                {
                    InsertItem(Count, itemKey, value);
                    return;
                }

                var index = IndexOf(key, item);
                if (index < 0)
                {
                    _dictionary.Remove(key);
                    InsertItem(Count, itemKey, value);
                    return;
                }

                SetItem(index, key, itemKey, value);
            }
        }
    }

    /// <summary>
    /// Gets a collection containing the dictionary keys.
    /// </summary>
    public global::System.Collections.Generic.ICollection<TKey> Keys
    {
        get
        {
            lock (_syncRoot)
            {
                return _dictionary.Keys;
            }
        }
    }

    /// <summary>
    /// Gets a collection containing the dictionary values.
    /// </summary>
    public global::System.Collections.Generic.ICollection<TValue> Values
    {
        get
        {
            lock (_syncRoot)
            {
                return _dictionary.Values;
            }
        }
    }

    /// <summary>
    /// Removes all items from the collection and dictionary.
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

    /// <summary>
    /// Inserts an item into the collection and dictionary at the specified index.
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

    /// <summary>
    /// Removes the item at the specified index from the collection and dictionary.
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
    /// Replaces the item at the specified index and keeps the dictionary key mapping synchronized.
    /// </summary>
    /// <param name="index">The zero-based index of the item to replace.</param>
    /// <param name="item">The replacement item.</param>
    protected override void SetItem(int index, TValue item)
    {
        lock (_syncRoot)
        {
            SetItem(index, _getKey(Items[index]), _getKey(item), item);
        }
    }

    private bool Contains(global::System.Collections.Generic.KeyValuePair<TKey, TValue> item)
    {
        return _dictionary.TryGetValue(item.Key, out var value) &&
               ValueComparer.Equals(value, item.Value);
    }

    private TKey GetValidatedItemKey(TKey key, TValue value)
    {
        var itemKey = _getKey(value);
        if (!KeyComparer.Equals(key, itemKey))
        {
            throw new global::System.ArgumentException("The key must match the key provided by the value.", nameof(key));
        }

        return itemKey;
    }

    private void InsertItem(int index, TKey key, TValue item)
    {
        CheckReentrancy();
        if ((uint)index > (uint)Count)
        {
            throw new global::System.ArgumentOutOfRangeException(nameof(index));
        }

        _dictionary.Add(key, item);
        base.InsertItem(index, item);
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
            throw new global::System.ArgumentException("An item with the same key already exists.", nameof(item));
        }

        _dictionary.Remove(oldKey);
        _dictionary.Add(newKey, item);
        base.SetItem(index, item);
    }

    private int IndexOf(TKey key, TValue value)
    {
        for (var index = 0; index < Items.Count; index++)
        {
            var item = Items[index];
            if (ReferenceEquals(item, value))
            {
                return index;
            }

            if (KeyComparer.Equals(_getKey(item), key) && ValueComparer.Equals(item, value))
            {
                return index;
            }
        }

        return -1;
    }

    private void RemoveDictionaryEntry(TValue item)
    {
        var key = _getKey(item);
        if (_dictionary.TryGetValue(key, out var value) && ValueComparer.Equals(value, item))
        {
            _dictionary.Remove(key);
            return;
        }

        TKey? keyToRemove = default;
        var shouldRemove = false;
        foreach (var pair in _dictionary)
        {
            if (ReferenceEquals(pair.Value, item))
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
}
