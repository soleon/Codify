using System.Collections;
using Codify.System.Collections.ObjectModel;

namespace Codify.System.Tests.Collections.ObjectModel;

public class ObservableDictionaryTests
{
    [Fact]
    public void AddWithMatchingKeyAddsDictionaryEntryAndCollectionItem()
    {
        var dictionary = CreateDictionary();
        var item = new TestItem(1, "one");

        dictionary.Add(1, item);

        Assert.True(dictionary.ContainsKey(1));
        Assert.Same(item, dictionary[1]);
        Assert.Collection((IEnumerable<TestItem>)dictionary, value => Assert.Same(item, value));
        Assert.Collection(dictionary.Keys, key => Assert.Equal(1, key));
        Assert.Collection(dictionary.Values, value => Assert.Same(item, value));
    }

    [Fact]
    public void AddThroughCollectionAddsDictionaryEntry()
    {
        var dictionary = CreateDictionary();
        var item = new TestItem(1, "one");

        dictionary.Add(item);

        Assert.True(dictionary.TryGetValue(1, out var value));
        Assert.Same(item, value);
        Assert.Same(item, ((IEnumerable<TestItem>)dictionary).Single());
    }

    [Fact]
    public void AddWithDuplicateKeyThrowsAndDoesNotAppendCollectionItem()
    {
        var dictionary = CreateDictionary();
        var first = new TestItem(1, "one");
        var duplicate = new TestItem(1, "duplicate");
        dictionary.Add(first);

        Assert.Throws<ArgumentException>(() => dictionary.Add(1, duplicate));

        Assert.Single(dictionary);
        Assert.Same(first, GetValueAt(dictionary, 0));
        Assert.Same(first, dictionary[1]);
    }

    [Fact]
    public void AddWithMismatchedKeyThrowsAndDoesNotMutate()
    {
        var dictionary = CreateDictionary();
        var item = new TestItem(1, "one");

        Assert.Throws<ArgumentException>(() => dictionary.Add(2, item));

        Assert.Empty(dictionary);
        Assert.False(dictionary.ContainsKey(1));
        Assert.False(dictionary.ContainsKey(2));
    }

    [Fact]
    public void RemoveKeyRemovesOnlyItemWithMatchingKeyWhenValuesCompareEqual()
    {
        var dictionary = CreateDictionary<EqualByGroupItem>();
        var first = new EqualByGroupItem(1, "same");
        var second = new EqualByGroupItem(2, "same");
        dictionary.Add(first);
        dictionary.Add(second);

        var removed = dictionary.Remove(2);

        Assert.True(removed);
        Assert.True(dictionary.ContainsKey(1));
        Assert.False(dictionary.ContainsKey(2));
        Assert.Collection((IEnumerable<EqualByGroupItem>)dictionary, value => Assert.Same(first, value));
        Assert.Same(first, dictionary[1]);
    }

    [Fact]
    public void RemoveKeyUsesStoredReferenceWhenEqualItemKeyMutatesToRemovedKey()
    {
        var dictionary = CreateDictionary<MutableEqualByGroupItem>();
        var first = new MutableEqualByGroupItem(1, "same");
        var second = new MutableEqualByGroupItem(2, "same");
        dictionary.Add(first);
        dictionary.Add(second);
        first.Key = 2;

        var removed = dictionary.Remove(2);

        Assert.True(removed);
        Assert.True(dictionary.ContainsKey(1));
        Assert.False(dictionary.ContainsKey(2));
        Assert.Collection((IEnumerable<MutableEqualByGroupItem>)dictionary, value => Assert.Same(first, value));
        Assert.Same(first, dictionary[1]);
    }

    [Fact]
    public void RemoveAtRemovesStoredReferenceWhenEqualItemKeyMutatesToAnotherKey()
    {
        var dictionary = CreateDictionary<MutableEqualByGroupItem>();
        var first = new MutableEqualByGroupItem(1, "same");
        var second = new MutableEqualByGroupItem(2, "same");
        dictionary.Add(first);
        dictionary.Add(second);
        first.Key = 2;

        dictionary.RemoveAt(0);

        Assert.False(dictionary.ContainsKey(1));
        Assert.True(dictionary.ContainsKey(2));
        Assert.Collection((IEnumerable<MutableEqualByGroupItem>)dictionary, value => Assert.Same(second, value));
        Assert.Same(second, dictionary[2]);
    }

    [Fact]
    public void RemoveMissingKeyReturnsFalseAndDoesNotMutate()
    {
        var dictionary = CreateDictionary();
        var item = new TestItem(1, "one");
        dictionary.Add(item);

        var removed = dictionary.Remove(2);

        Assert.False(removed);
        Assert.Single(dictionary);
        Assert.Same(item, dictionary[1]);
    }

    [Fact]
    public void RemovePairRemovesOnlyWhenKeyAndValueMatch()
    {
        var dictionary = CreateDictionary();
        var item = new TestItem(1, "one");
        dictionary.Add(item);
        var pairs = (ICollection<KeyValuePair<int, TestItem>>)dictionary;

        var removedMismatchedValue = pairs.Remove(new KeyValuePair<int, TestItem>(1, new TestItem(1, "different")));
        var removedMismatchedKey = pairs.Remove(new KeyValuePair<int, TestItem>(2, item));
        var removed = pairs.Remove(new KeyValuePair<int, TestItem>(1, item));

        Assert.False(removedMismatchedValue);
        Assert.False(removedMismatchedKey);
        Assert.True(removed);
        Assert.Empty(dictionary);
        Assert.False(dictionary.ContainsKey(1));
    }

    [Fact]
    public void ContainsPairRequiresMatchingKeyAndValue()
    {
        var dictionary = CreateDictionary();
        var item = new TestItem(1, "one");
        dictionary.Add(item);
        var pairs = (ICollection<KeyValuePair<int, TestItem>>)dictionary;

        Assert.Contains(new KeyValuePair<int, TestItem>(1, item), pairs);
        Assert.DoesNotContain(new KeyValuePair<int, TestItem>(1, new TestItem(1, "different")), pairs);
        Assert.DoesNotContain(new KeyValuePair<int, TestItem>(2, item), pairs);
    }

    [Fact]
    public void DictionaryIndexerUpdatesExistingCollectionItem()
    {
        var dictionary = CreateDictionary();
        var first = new TestItem(1, "one");
        var replacement = new TestItem(1, "replacement");
        dictionary.Add(first);

        dictionary[1] = replacement;

        Assert.Single(dictionary);
        Assert.Same(replacement, GetValueAt(dictionary, 0));
        Assert.Same(replacement, dictionary[1]);
    }

    [Fact]
    public void DictionaryIndexerAddsNewMatchingKeyWhenKeyDoesNotExist()
    {
        var dictionary = CreateDictionary();
        var item = new TestItem(1, "one");

        dictionary[1] = item;

        Assert.Single(dictionary);
        Assert.Same(item, GetValueAt(dictionary, 0));
        Assert.Same(item, dictionary[1]);
    }

    [Fact]
    public void DictionaryIndexerRejectsMismatchedKeyAndDoesNotMutate()
    {
        var dictionary = CreateDictionary();
        var existing = new TestItem(1, "one");
        dictionary.Add(existing);

        Assert.Throws<ArgumentException>(() => dictionary[1] = new TestItem(2, "two"));

        Assert.Single(dictionary);
        Assert.Same(existing, GetValueAt(dictionary, 0));
        Assert.Same(existing, dictionary[1]);
        Assert.False(dictionary.ContainsKey(2));
    }

    [Fact]
    public void CollectionIndexerChangingItemKeyMovesDictionaryKey()
    {
        var dictionary = CreateDictionary();
        var first = new TestItem(1, "one");
        var replacement = new TestItem(2, "two");
        dictionary.Add(first);

        SetValueAt(dictionary, 0, replacement);

        Assert.False(dictionary.ContainsKey(1));
        Assert.True(dictionary.ContainsKey(2));
        Assert.Same(replacement, GetValueAt(dictionary, 0));
        Assert.Same(replacement, dictionary[2]);
    }

    [Fact]
    public void CollectionIndexerReplacingMutatedItemKeyRemovesOriginalDictionaryEntry()
    {
        var dictionary = CreateDictionary<MutableKeyItem>();
        var item = new MutableKeyItem(1, "one");
        dictionary.Add(item);
        item.Key = 2;
        var replacement = new MutableKeyItem(3, "three");

        SetValueAt(dictionary, 0, replacement);

        Assert.False(dictionary.ContainsKey(1));
        Assert.False(dictionary.ContainsKey(2));
        Assert.True(dictionary.ContainsKey(3));
        Assert.Same(replacement, dictionary[3]);
        Assert.Collection((IEnumerable<MutableKeyItem>)dictionary, value => Assert.Same(replacement, value));
    }

    [Fact]
    public void CollectionIndexerChangingToExistingKeyThrowsAndDoesNotMutate()
    {
        var dictionary = CreateDictionary();
        var first = new TestItem(1, "one");
        var second = new TestItem(2, "two");
        dictionary.Add(first);
        dictionary.Add(second);

        Assert.Throws<ArgumentException>(() => SetValueAt(dictionary, 0, new TestItem(2, "replacement")));

        Assert.Equal(2, dictionary.Count);
        Assert.Same(first, GetValueAt(dictionary, 0));
        Assert.Same(second, GetValueAt(dictionary, 1));
        Assert.Same(first, dictionary[1]);
        Assert.Same(second, dictionary[2]);
    }

    [Fact]
    public void RemoveAtRemovesDictionaryEntry()
    {
        var dictionary = CreateDictionary();
        dictionary.Add(new TestItem(1, "one"));
        var second = new TestItem(2, "two");
        dictionary.Add(second);

        dictionary.RemoveAt(0);

        Assert.False(dictionary.ContainsKey(1));
        Assert.True(dictionary.ContainsKey(2));
        Assert.Collection((IEnumerable<TestItem>)dictionary, value => Assert.Same(second, value));
    }

    [Fact]
    public void ClearRemovesCollectionItemsAndDictionaryEntries()
    {
        var dictionary = CreateDictionary();
        dictionary.Add(new TestItem(1, "one"));
        dictionary.Add(new TestItem(2, "two"));

        dictionary.Clear();

        Assert.Empty(dictionary);
        Assert.Empty(dictionary.Keys);
        Assert.Empty(dictionary.Values);
        Assert.False(dictionary.ContainsKey(1));
        Assert.False(dictionary.ContainsKey(2));
    }

    [Fact]
    public void RejectedReentrantRemoveAtDoesNotChangeDictionaryEntries()
    {
        var dictionary = CreateDictionary();
        var first = new TestItem(1, "one");
        var second = new TestItem(2, "two");
        dictionary.Add(first);
        Exception? reentrantException = null;
        dictionary.CollectionChanged += (_, _) =>
        {
            reentrantException ??= Record.Exception(() => dictionary.RemoveAt(0));
        };
        dictionary.CollectionChanged += (_, _) => { };

        dictionary.Add(second);

        Assert.IsType<InvalidOperationException>(reentrantException);
        Assert.Equal(2, dictionary.Count);
        Assert.Same(first, GetValueAt(dictionary, 0));
        Assert.Same(second, GetValueAt(dictionary, 1));
        Assert.Same(first, dictionary[1]);
        Assert.Same(second, dictionary[2]);
    }

    [Fact]
    public void RejectedReentrantClearDoesNotChangeDictionaryEntries()
    {
        var dictionary = CreateDictionary();
        var first = new TestItem(1, "one");
        var second = new TestItem(2, "two");
        dictionary.Add(first);
        Exception? reentrantException = null;
        dictionary.CollectionChanged += (_, _) =>
        {
            reentrantException ??= Record.Exception(dictionary.Clear);
        };
        dictionary.CollectionChanged += (_, _) => { };

        dictionary.Add(second);

        Assert.IsType<InvalidOperationException>(reentrantException);
        Assert.Equal(2, dictionary.Count);
        Assert.Same(first, GetValueAt(dictionary, 0));
        Assert.Same(second, GetValueAt(dictionary, 1));
        Assert.Same(first, dictionary[1]);
        Assert.Same(second, dictionary[2]);
    }

    [Fact]
    public void RejectedReentrantSameKeySetDoesNotChangeDictionaryEntry()
    {
        var dictionary = CreateDictionary();
        var first = new TestItem(1, "one");
        var second = new TestItem(2, "two");
        dictionary.Add(first);
        Exception? reentrantException = null;
        dictionary.CollectionChanged += (_, _) =>
        {
            reentrantException ??= Record.Exception(() => SetValueAt(dictionary, 0, new TestItem(1, "replacement")));
        };
        dictionary.CollectionChanged += (_, _) => { };

        dictionary.Add(second);

        Assert.IsType<InvalidOperationException>(reentrantException);
        Assert.Equal(2, dictionary.Count);
        Assert.Same(first, GetValueAt(dictionary, 0));
        Assert.Same(second, GetValueAt(dictionary, 1));
        Assert.Same(first, dictionary[1]);
        Assert.Same(second, dictionary[2]);
    }

    [Fact]
    public void CopyToCopiesDictionaryPairs()
    {
        var dictionary = CreateDictionary();
        var item = new TestItem(1, "one");
        dictionary.Add(item);
        var pairs = (ICollection<KeyValuePair<int, TestItem>>)dictionary;
        var target = new KeyValuePair<int, TestItem>[1];

        pairs.CopyTo(target, 0);

        var pair = Assert.Single(target);
        Assert.Equal(1, pair.Key);
        Assert.Same(item, pair.Value);
    }

    [Fact]
    public void NonGenericEnumeratorEnumeratesCollectionItems()
    {
        var dictionary = CreateDictionary();
        var item = new TestItem(1, "one");
        dictionary.Add(item);

        var enumerator = ((IEnumerable)dictionary).GetEnumerator();

        Assert.True(enumerator.MoveNext());
        Assert.Same(item, enumerator.Current);
        Assert.False(enumerator.MoveNext());
    }

    [Fact]
    public void ConstructorRejectsNullKeyProvider()
    {
        Assert.Throws<ArgumentNullException>(() => new ObservableDictionary<int, TestItem>(null!));
    }

    [Fact]
    public void IsReadOnlyAlwaysReturnsFalse()
    {
        var dictionary = CreateDictionary();

        Assert.False(dictionary.IsReadOnly);
        Assert.False(((ICollection<KeyValuePair<int, TestItem>>)dictionary).IsReadOnly);
    }

    private static ObservableDictionary<int, TItem> CreateDictionary<TItem>()
        where TItem : IKeyedItem
    {
        return new ObservableDictionary<int, TItem>(item => item.Key);
    }

    private static ObservableDictionary<int, TestItem> CreateDictionary()
    {
        return CreateDictionary<TestItem>();
    }

    private static TItem GetValueAt<TItem>(ObservableDictionary<int, TItem> dictionary, int index)
    {
        return ((IList<TItem>)dictionary)[index];
    }

    private static void SetValueAt<TItem>(ObservableDictionary<int, TItem> dictionary, int index, TItem item)
    {
        ((IList<TItem>)dictionary)[index] = item;
    }

    private interface IKeyedItem
    {
        int Key { get; }
    }

    private sealed record TestItem(int Key, string Value) : IKeyedItem;

    private sealed class MutableKeyItem : IKeyedItem
    {
        public MutableKeyItem(int key, string value)
        {
            Key = key;
            Value = value;
        }

        public int Key { get; set; }

        public string Value { get; }
    }

    private sealed class EqualByGroupItem : IKeyedItem
    {
        private readonly string _group;

        public EqualByGroupItem(int key, string group)
        {
            Key = key;
            _group = group;
        }

        public int Key { get; }

        public override bool Equals(object? obj)
        {
            return obj is EqualByGroupItem item && _group == item._group;
        }

        public override int GetHashCode()
        {
            return _group.GetHashCode(StringComparison.Ordinal);
        }
    }

    private sealed class MutableEqualByGroupItem : IKeyedItem
    {
        private readonly string _group;

        public MutableEqualByGroupItem(int key, string group)
        {
            Key = key;
            _group = group;
        }

        public int Key { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is MutableEqualByGroupItem item && _group == item._group;
        }

        public override int GetHashCode()
        {
            return _group.GetHashCode(StringComparison.Ordinal);
        }
    }
}
