using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using Codify.System.Collections.ObjectModel;

namespace Codify.System.Tests.Collections.ObjectModel;

public class AdaptiveObservableCollectionTests
{
    [Fact]
    public void ConstructorCopiesExistingSourceItems()
    {
        var source = new RangeObservableCollection<int> { 1, 2 };

        var collection = CreateCollection(source);

        Assert.Equal(["1", "2"], collection);
    }

    [Fact]
    public void SingleItemChangesStaySynchronized()
    {
        var source = new RangeObservableCollection<int> { 1, 3 };
        var collection = CreateCollection(source);

        source.Insert(1, 2);
        source.RemoveAt(1);
        source[1] = 4;
        source.Move(1, 0);

        Assert.Equal(["4", "1"], collection);
    }

    [Fact]
    public void MultiItemAddPreservesSourceOrderAtStartingIndex()
    {
        var source = new RangeObservableCollection<int> { 1, 4 };
        var collection = CreateCollection(source);

        source.InsertRange(1, 2, 3);

        Assert.Equal(["1", "2", "3", "4"], collection);
    }

    [Fact]
    public void MultiItemRemoveRemovesExactlyOldItems()
    {
        var source = new RangeObservableCollection<int> { 1, 2, 3, 4 };
        var collection = CreateCollection(source);

        source.RemoveRange(1, 2);

        Assert.Equal(["1", "4"], collection);
    }

    [Fact]
    public void MultiItemReplaceUpdatesAllChangedItems()
    {
        var source = new RangeObservableCollection<int> { 1, 2, 3, 4 };
        var collection = CreateCollection(source);

        source.ReplaceRange(1, 5, 6);

        Assert.Equal(["1", "5", "6", "4"], collection);
    }

    [Fact]
    public void MultiItemMoveMovesAllChangedItems()
    {
        var source = new RangeObservableCollection<int> { 1, 2, 3, 4, 5 };
        var collection = CreateCollection(source);

        source.MoveRange(1, 2, 3);

        Assert.Equal(["1", "4", "5", "2", "3"], collection);
    }

    [Fact]
    public void ResetReloadsFromSource()
    {
        var source = new RangeObservableCollection<int> { 1, 2 };
        var collection = CreateCollection(source);

        source.ResetTo(4, 5, 6);

        Assert.Equal(["4", "5", "6"], collection);
    }

    [Fact]
    public void ConstructorRejectsNullArguments()
    {
        var source = new ObservableCollection<int>();

        Assert.Throws<ArgumentNullException>(() => new AdaptiveObservableCollection<int, string>(null!, Convert));
        Assert.Throws<ArgumentNullException>(() => new AdaptiveObservableCollection<int, string>(source, null!));
    }

    [Fact]
    public void DisposeUnsubscribesAndClearsItems()
    {
        var source = new RangeObservableCollection<int> { 1 };
        var collection = CreateCollection(source);

        collection.Dispose();
        source.InsertRange(1, 2);

        Assert.Empty(collection);
    }

    [Fact]
    public void DisposeIsIdempotent()
    {
        var source = new RangeObservableCollection<int> { 1, 2 };
        var collection = CreateCollection(source);

        collection.Dispose();
        collection.Dispose();

        Assert.Empty(collection);
    }

    [Fact]
    public void NegativeAddStartingIndexReloadsFromSource()
    {
        var source = new IndexedNotifyingObservableCollection<int> { 1, 2 };
        var collection = CreateCollection(source);

        source.RaiseChange(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Add,
            changedItems: new List<int> { 99 },
            startingIndex: -1));

        Assert.Equal(["1", "2"], collection);
    }

    [Fact]
    public void NegativeRemoveStartingIndexReloadsFromSource()
    {
        var source = new IndexedNotifyingObservableCollection<int> { 1, 2 };
        var collection = CreateCollection(source);

        source.RaiseChange(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Remove,
            changedItems: new List<int> { 1 },
            startingIndex: -1));

        Assert.Equal(["1", "2"], collection);
    }

    [Fact]
    public void NegativeReplaceStartingIndexReloadsFromSource()
    {
        var source = new IndexedNotifyingObservableCollection<int> { 1, 2 };
        var collection = CreateCollection(source);

        source.RaiseChange(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Replace,
            newItems: new List<int> { 99 },
            oldItems: new List<int> { 1 },
            startingIndex: -1));

        Assert.Equal(["1", "2"], collection);
    }

    // The Move negative-index branch in MoveItems is defensive against custom
    // NotifyCollectionChangedEventArgs sources that bypass standard constructor validation.
    // The public Move constructors all reject negative indices, so it is not reachable through
    // any public BCL API and is intentionally not exercised by tests.

    private static AdaptiveObservableCollection<int, string> CreateCollection(ObservableCollection<int> source)
    {
        return new AdaptiveObservableCollection<int, string>(source, Convert);
    }

    private static string Convert(int value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }

    private sealed class IndexedNotifyingObservableCollection<T> : ObservableCollection<T>
    {
        public void RaiseChange(NotifyCollectionChangedEventArgs args)
        {
            OnCollectionChanged(args);
        }
    }

    private sealed class RangeObservableCollection<T> : ObservableCollection<T>
    {
        public void InsertRange(int index, params T[] items)
        {
            foreach (var item in items)
            {
                Items.Insert(index++, item);
            }

            OnCountChanged();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Add,
                items,
                index - items.Length));
        }

        public void RemoveRange(int index, int count)
        {
            var removedItems = new List<T>(count);
            for (var i = 0; i < count; i++)
            {
                removedItems.Add(Items[index]);
                Items.RemoveAt(index);
            }

            OnCountChanged();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Remove,
                removedItems,
                index));
        }

        public void ReplaceRange(int index, params T[] items)
        {
            var oldItems = new List<T>(items.Length);
            for (var i = 0; i < items.Length; i++)
            {
                oldItems.Add(Items[index + i]);
                Items[index + i] = items[i];
            }

            OnIndexerChanged();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Replace,
                items,
                oldItems,
                index));
        }

        public void MoveRange(int oldIndex, int count, int newIndex)
        {
            var movedItems = new List<T>(count);
            for (var i = 0; i < count; i++)
            {
                movedItems.Add(Items[oldIndex]);
                Items.RemoveAt(oldIndex);
            }

            for (var i = 0; i < movedItems.Count; i++)
            {
                Items.Insert(newIndex + i, movedItems[i]);
            }

            OnIndexerChanged();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Move,
                movedItems,
                newIndex,
                oldIndex));
        }

        public void ResetTo(params T[] items)
        {
            Items.Clear();
            foreach (var item in items)
            {
                Items.Add(item);
            }

            OnCountChanged();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private void OnCountChanged()
        {
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            OnIndexerChanged();
        }

        private void OnIndexerChanged()
        {
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        }
    }
}
