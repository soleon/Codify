using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Codify.System.Collections.ObjectModel;

/// <summary>
///     A read only observable collection that synchronises to a source collection of a different item type.
/// </summary>
/// <typeparam name="TSource"> The type of the items in the source collection.</typeparam>
/// <typeparam name="TTarget"> The type of the items in this collection.</typeparam>
public class AdaptiveObservableCollection<TSource, TTarget> : ObservableCollection<TTarget>, IDisposable
{
    /// <summary>
    ///     A reference to the function used to wrap source items.
    /// </summary>
    private readonly Func<TSource, TTarget> _converter;

    /// <summary>
    ///     An internal list that keep track of the exact source items that maps 1 to 1 to target items in this collection.
    /// </summary>
    private readonly ObservableCollection<TSource> _sourceCollection;

    /// <summary>
    ///     Creates a new instance of <see cref="AdaptiveObservableCollection{TSource,TTarget}" />.
    /// </summary>
    /// <param name="sourceCollection">
    ///     The source collection to adapt to.
    /// </param>
    /// <param name="converter">
    ///     The function used to convert <typeparamref name="TSource" /> items to <typeparamref name="TTarget" /> items.
    /// </param>
    public AdaptiveObservableCollection(ObservableCollection<TSource> sourceCollection,
        Func<TSource, TTarget> converter)
    {
        ArgumentNullException.ThrowIfNull(sourceCollection);
        ArgumentNullException.ThrowIfNull(converter);

        _sourceCollection = sourceCollection;
        _converter = converter;

        Reload();
        _sourceCollection.CollectionChanged += OnSourceCollectionChanged;
    }

    /// <summary>
    ///     Clears existing items and stops synchronising with the source collection.
    /// </summary>
    public void Dispose()
    {
        _sourceCollection.CollectionChanged -= OnSourceCollectionChanged;
        ClearItems();
        global::System.GC.SuppressFinalize(this);
    }

    private void OnSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                ArgumentNullException.ThrowIfNull(e.NewItems);
                InsertItems(e.NewStartingIndex, e.NewItems);
                break;
            case NotifyCollectionChangedAction.Remove:
                ArgumentNullException.ThrowIfNull(e.OldItems);
                RemoveItems(e.OldStartingIndex, e.OldItems);
                break;
            case NotifyCollectionChangedAction.Replace:
                ArgumentNullException.ThrowIfNull(e.NewItems);
                ArgumentNullException.ThrowIfNull(e.OldItems);
                ReplaceItems(e.NewStartingIndex, e.OldItems, e.NewItems);
                break;
            case NotifyCollectionChangedAction.Reset:
                ResetFromSource();
                break;
            case NotifyCollectionChangedAction.Move:
                ArgumentNullException.ThrowIfNull(e.NewItems);
                MoveItems(e.OldStartingIndex, e.NewStartingIndex, e.NewItems);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(e), e.Action, "Unsupported collection change action.");
        }
    }

    private void InsertItems(int startIndex, global::System.Collections.IList newItems)
    {
        if (startIndex < 0)
        {
            ResetFromSource();
            return;
        }

        var insertIndex = startIndex;
        foreach (TSource item in newItems)
        {
            InsertItem(insertIndex >= Count ? Count : insertIndex, _converter(item));
            insertIndex++;
        }
    }

    private void RemoveItems(int startIndex, global::System.Collections.IList oldItems)
    {
        if (startIndex < 0)
        {
            ResetFromSource();
            return;
        }

        foreach (var _ in oldItems)
        {
            RemoveItem(startIndex);
        }
    }

    private void ReplaceItems(
        int startIndex,
        global::System.Collections.IList oldItems,
        global::System.Collections.IList newItems)
    {
        if (startIndex < 0)
        {
            ResetFromSource();
            return;
        }

        if (oldItems.Count == newItems.Count)
        {
            var replaceIndex = startIndex;
            foreach (TSource item in newItems)
            {
                SetItem(replaceIndex, _converter(item));
                replaceIndex++;
            }

            return;
        }

        foreach (var _ in oldItems)
        {
            RemoveItem(startIndex);
        }

        InsertItems(startIndex, newItems);
    }

    private void MoveItems(int oldIndex, int newIndex, global::System.Collections.IList movedItems)
    {
        if (oldIndex < 0 || newIndex < 0)
        {
            ResetFromSource();
            return;
        }

        if (movedItems.Count == 1)
        {
            MoveItem(oldIndex, newIndex);
            return;
        }

        foreach (var _ in movedItems)
        {
            RemoveItem(oldIndex);
        }

        InsertItems(newIndex, movedItems);
    }

    private void ResetFromSource()
    {
        ClearItems();
        Reload();
    }

    private void Reload()
    {
        foreach (var sourceItem in _sourceCollection)
        {
            Add(_converter(sourceItem));
        }
    }
}
