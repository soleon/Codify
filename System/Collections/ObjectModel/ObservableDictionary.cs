namespace Codify.System.Collections.ObjectModel;

public class ObservableDictionary<TKey, TValue> : BatchObservableCollection<TValue>, IDictionary<TKey, TValue>
{
    private static readonly EqualityComparer<TKey> KeyComparer = EqualityComparer<TKey>.Default;
    private static readonly EqualityComparer<TValue> ValueComparer = EqualityComparer<TValue>.Default;

    private readonly Dictionary<TKey, TValue> _dictionary = new();
    private readonly Func<TValue, TKey> _getKey;

    public ObservableDictionary(Func<TValue, TKey> keyProvider)
    {
        ArgumentNullException.ThrowIfNull(keyProvider);

        _getKey = keyProvider;
    }

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
    {
        return _dictionary.GetEnumerator();
    }

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
    {
        lock (_dictionary)
        {
            Add(item.Key, item.Value);
        }
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
    {
        lock (_dictionary)
        {
            return Contains(item);
        }
    }

    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        lock (_dictionary)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).CopyTo(array, arrayIndex);
        }
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
        lock (_dictionary)
        {
            return Contains(item) && Remove(item.Key);
        }
    }

    public bool IsReadOnly { get; }

    public void Add(TKey key, TValue value)
    {
        lock (_dictionary)
        {
            var itemKey = GetValidatedItemKey(key, value);
            InsertItem(Count, itemKey, value);
        }
    }

    public bool ContainsKey(TKey key)
    {
        lock (_dictionary)
        {
            return _dictionary.ContainsKey(key);
        }
    }

    public bool Remove(TKey key)
    {
        lock (_dictionary)
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

    public bool TryGetValue(TKey key, out TValue value)
    {
        lock (_dictionary)
        {
            return _dictionary.TryGetValue(key, out value);
        }
    }

    public TValue this[TKey key]
    {
        get
        {
            lock (_dictionary)
            {
                return _dictionary[key];
            }
        }
        set
        {
            lock (_dictionary)
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

    public ICollection<TKey> Keys
    {
        get
        {
            lock (_dictionary)
            {
                return _dictionary.Keys;
            }
        }
    }

    public ICollection<TValue> Values
    {
        get
        {
            lock (_dictionary)
            {
                return _dictionary.Values;
            }
        }
    }

    protected override void ClearItems()
    {
        lock (_dictionary)
        {
            _dictionary.Clear();
            base.ClearItems();
        }
    }

    protected override void InsertItem(int index, TValue item)
    {
        lock (_dictionary)
        {
            InsertItem(index, _getKey(item), item);
        }
    }

    protected override void RemoveItem(int index)
    {
        lock (_dictionary)
        {
            RemoveDictionaryEntry(Items[index]);
            base.RemoveItem(index);
        }
    }

    protected override void SetItem(int index, TValue item)
    {
        lock (_dictionary)
        {
            SetItem(index, _getKey(Items[index]), _getKey(item), item);
        }
    }

    private bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return _dictionary.TryGetValue(item.Key, out var value) &&
               ValueComparer.Equals(value, item.Value);
    }

    private TKey GetValidatedItemKey(TKey key, TValue value)
    {
        var itemKey = _getKey(value);
        if (!KeyComparer.Equals(key, itemKey))
        {
            throw new ArgumentException("The key must match the key provided by the value.", nameof(key));
        }

        return itemKey;
    }

    private void InsertItem(int index, TKey key, TValue item)
    {
        _dictionary.Add(key, item);

        try
        {
            base.InsertItem(index, item);
        }
        catch
        {
            _dictionary.Remove(key);
            throw;
        }
    }

    private void SetItem(int index, TKey oldKey, TKey newKey, TValue item)
    {
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

        var oldItem = Items[index];
        _dictionary.Remove(oldKey);
        _dictionary.Add(newKey, item);

        try
        {
            base.SetItem(index, item);
        }
        catch
        {
            _dictionary.Remove(newKey);
            _dictionary[oldKey] = oldItem;
            throw;
        }
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

        var keyToRemove = default(TKey);
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
            _dictionary.Remove(keyToRemove);
        }
    }
}
