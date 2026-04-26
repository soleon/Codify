namespace Codify.System.Collections.ObjectModel;

public class ObservableDictionary<TKey, TValue> : BatchObservableCollection<TValue>, IDictionary<TKey, TValue>
    where TKey : notnull
{
    private static readonly EqualityComparer<TKey> KeyComparer = EqualityComparer<TKey>.Default;
    private static readonly EqualityComparer<TValue> ValueComparer = EqualityComparer<TValue>.Default;

    private readonly Dictionary<TKey, TValue> _dictionary = new();
    private readonly Func<TValue, TKey> _getKey;
    private readonly global::System.Threading.Lock _syncRoot = new();

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
        lock (_syncRoot)
        {
            Add(item.Key, item.Value);
        }
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
    {
        lock (_syncRoot)
        {
            return Contains(item);
        }
    }

    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        lock (_syncRoot)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).CopyTo(array, arrayIndex);
        }
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
        lock (_syncRoot)
        {
            return Contains(item) && Remove(item.Key);
        }
    }

    public bool IsReadOnly { get; }

    public void Add(TKey key, TValue value)
    {
        lock (_syncRoot)
        {
            var itemKey = GetValidatedItemKey(key, value);
            InsertItem(Count, itemKey, value);
        }
    }

    public bool ContainsKey(TKey key)
    {
        lock (_syncRoot)
        {
            return _dictionary.ContainsKey(key);
        }
    }

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

    public bool TryGetValue(
        TKey key,
        [global::System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out TValue value)
    {
        lock (_syncRoot)
        {
            return _dictionary.TryGetValue(key, out value);
        }
    }

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

    public ICollection<TKey> Keys
    {
        get
        {
            lock (_syncRoot)
            {
                return _dictionary.Keys;
            }
        }
    }

    public ICollection<TValue> Values
    {
        get
        {
            lock (_syncRoot)
            {
                return _dictionary.Values;
            }
        }
    }

    protected override void ClearItems()
    {
        lock (_syncRoot)
        {
            CheckReentrancy();
            _dictionary.Clear();
            base.ClearItems();
        }
    }

    protected override void InsertItem(int index, TValue item)
    {
        lock (_syncRoot)
        {
            InsertItem(index, _getKey(item), item);
        }
    }

    protected override void RemoveItem(int index)
    {
        lock (_syncRoot)
        {
            CheckReentrancy();
            RemoveDictionaryEntry(Items[index]);
            base.RemoveItem(index);
        }
    }

    protected override void SetItem(int index, TValue item)
    {
        lock (_syncRoot)
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
        CheckReentrancy();
        if ((uint)index > (uint)Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
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
            throw new ArgumentException("An item with the same key already exists.", nameof(item));
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
