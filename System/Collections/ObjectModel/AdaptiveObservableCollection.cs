namespace Codify.System.Collections.ObjectModel;

/// <summary>
/// An observable collection that synchronises to a source collection of a different item type.
/// </summary>
/// <typeparam name="TSource"> The type of the items in the source collection.</typeparam>
/// <typeparam name="TTarget"> The type of the items in this collection.</typeparam>
public class AdaptiveObservableCollection<TSource, TTarget> :
    global::System.Collections.ObjectModel.ObservableCollection<TTarget>,
    global::System.IDisposable
{
    /// <summary>
    /// A reference to the function used to wrap source items.
    /// </summary>
    private readonly global::System.Func<TSource, TTarget> _converter;

    /// <summary>
    /// An internal list that keep track of the exact source items that maps 1 to 1 to target items in this collection.
    /// </summary>
    private readonly global::System.Collections.ObjectModel.ObservableCollection<TSource> _sourceCollection;

    private bool _isDisposed;

    /// <summary>
    /// Creates a new instance of <see cref="AdaptiveObservableCollection{TSource,TTarget}" />.
    /// </summary>
    /// <param name="sourceCollection">
    /// The source collection to adapt to.
    /// </param>
    /// <param name="converter">
    /// The function used to convert <typeparamref name="TSource" /> items to <typeparamref name="TTarget" /> items.
    /// </param>
    public AdaptiveObservableCollection(
        global::System.Collections.ObjectModel.ObservableCollection<TSource> sourceCollection,
        global::System.Func<TSource, TTarget> converter)
    {
        global::System.ArgumentNullException.ThrowIfNull(sourceCollection);
        global::System.ArgumentNullException.ThrowIfNull(converter);

        _sourceCollection = sourceCollection;
        _converter = converter;

        Reload();
        _sourceCollection.CollectionChanged += OnSourceCollectionChanged;
    }

    /// <summary>
    /// Clears existing items and stops synchronising with the source collection.
    /// </summary>
    /// <remarks>
    /// Subsequent calls are a no-op. Derived types should override <see cref="Dispose(bool)" /> to release
    /// additional resources rather than overriding this method.
    /// </remarks>
    public void Dispose()
    {
        Dispose(disposing: true);
        global::System.GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases resources held by this collection.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true" /> when invoked from <see cref="Dispose()" />; <see langword="false" /> when invoked from a finalizer.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        if (!disposing)
        {
            return;
        }

        _sourceCollection.CollectionChanged -= OnSourceCollectionChanged;
        ClearItems();
    }

    private void OnSourceCollectionChanged(
        object? sender,
        global::System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case global::System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                global::System.ArgumentNullException.ThrowIfNull(e.NewItems);
                InsertItems(e.NewStartingIndex, e.NewItems);
                break;
            case global::System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                global::System.ArgumentNullException.ThrowIfNull(e.OldItems);
                RemoveItems(e.OldStartingIndex, e.OldItems);
                break;
            case global::System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                global::System.ArgumentNullException.ThrowIfNull(e.NewItems);
                global::System.ArgumentNullException.ThrowIfNull(e.OldItems);
                ReplaceItems(e.NewStartingIndex, e.OldItems, e.NewItems);
                break;
            case global::System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                ResetFromSource();
                break;
            case global::System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                global::System.ArgumentNullException.ThrowIfNull(e.NewItems);
                MoveItems(e.OldStartingIndex, e.NewStartingIndex, e.NewItems);
                break;
            default:
                throw new global::System.ArgumentOutOfRangeException(
                    nameof(e),
                    e.Action,
                    "Unsupported collection change action.");
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
        if (newItems is global::System.Collections.Generic.IList<TSource> typedItems)
        {
            for (var i = 0; i < typedItems.Count; i++)
            {
                InsertItem(insertIndex >= Count ? Count : insertIndex, _converter(typedItems[i]));
                insertIndex++;
            }

            return;
        }

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

        for (var i = 0; i < oldItems.Count; i++)
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
            if (newItems is global::System.Collections.Generic.IList<TSource> typedItems)
            {
                for (var i = 0; i < typedItems.Count; i++)
                {
                    SetItem(startIndex + i, _converter(typedItems[i]));
                }

                return;
            }

            var replaceIndex = startIndex;
            foreach (TSource item in newItems)
            {
                SetItem(replaceIndex, _converter(item));
                replaceIndex++;
            }

            return;
        }

        for (var i = 0; i < oldItems.Count; i++)
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

        for (var i = 0; i < movedItems.Count; i++)
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
