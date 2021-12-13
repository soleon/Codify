using System;
using System.Collections.Generic;

namespace Codify.System.Collections.ObjectModel
{
    public class ObservableDictionary<TKey, TValue> : BatchObservableCollection<TValue>, IDictionary<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();
        private readonly Func<TValue, TKey> _getKey;

        public ObservableDictionary(Func<TValue, TKey> keyProvider)
        {
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
            return ContainsKey(item.Key);
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
                return Remove(item.Key) && Remove(item.Value);
            }
        }

        public bool IsReadOnly { get; } = false;

        public void Add(TKey key, TValue value)
        {
            lock (_dictionary)
            {
                _dictionary.Add(key, value);
                Add(value);
            }
        }

        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            lock (_dictionary)
            {
                if (_dictionary.TryGetValue(key, out var item))
                {
                    Remove(item);
                }

                return _dictionary.Remove(key);
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
                    if (_dictionary.TryGetValue(key, out var item))
                    {
                        base.SetItem(IndexOf(item), value);
                    }

                    _dictionary[key] = value;
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
                _dictionary[_getKey(item)] = item;
                base.InsertItem(index, item);
            }
        }

        protected override void RemoveItem(int index)
        {
            lock (_dictionary)
            {
                _dictionary.Remove(_getKey(this[index]));
                base.RemoveItem(index);
            }
        }

        protected override void SetItem(int index, TValue item)
        {
            lock (_dictionary)
            {
                _dictionary[_getKey(this[index])] = item;
                base.SetItem(index, item);
            }
        }
    }
}