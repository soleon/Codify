namespace Codify.System.Collections.ObjectModel;

/// <summary>
/// Provides an observable collection that can suppress intermediate notifications during a batch update.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
public class BatchObservableCollection<T> : global::System.Collections.ObjectModel.ObservableCollection<T>
{
    private readonly global::System.Threading.Lock _collectionUpdateLock = new();

    private readonly global::System.ComponentModel.PropertyChangedEventArgs _countPropertyChangedEventArgs = new(nameof(Count));

    private readonly global::System.ComponentModel.PropertyChangedEventArgs _indexerPropertyChangedEventArgs = new("Item[]");

    private int _updateDepth;

    /// <summary>
    /// Begins a batch update and returns a handle that ends the update when disposed.
    /// </summary>
    /// <returns>
    /// A disposable handle that raises a reset notification after the update completes.
    /// </returns>
    public global::System.IDisposable BeginUpdate()
    {
        lock (_collectionUpdateLock)
        {
            _updateDepth++;
        }

        return new CollectionUpdateHandle(this);
    }

    /// <summary>
    /// Raises a property change notification unless a batch update is active.
    /// </summary>
    /// <param name="e">The property change data.</param>
    protected override void OnPropertyChanged(global::System.ComponentModel.PropertyChangedEventArgs e)
    {
        lock (_collectionUpdateLock)
        {
            if (_updateDepth > 0) return;
        }

        base.OnPropertyChanged(e);
    }

    private void NotifyCollectionChanged(global::System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        base.OnCollectionChanged(e);
    }

    private void EndUpdate()
    {
        bool shouldNotify;

        lock (_collectionUpdateLock)
        {
            _updateDepth--;
            shouldNotify = _updateDepth == 0;
        }

        if (!shouldNotify) return;

        NotifyCollectionChanged(
            new global::System.Collections.Specialized.NotifyCollectionChangedEventArgs(
                global::System.Collections.Specialized.NotifyCollectionChangedAction.Reset));
        OnPropertyChanged(_countPropertyChangedEventArgs);
        OnPropertyChanged(_indexerPropertyChangedEventArgs);
    }

    /// <summary>
    /// Raises a collection change notification unless a batch update is active.
    /// </summary>
    /// <param name="e">The collection change data.</param>
    protected override void OnCollectionChanged(global::System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        lock (_collectionUpdateLock)
        {
            if (_updateDepth > 0) return;
        }

        NotifyCollectionChanged(e);
    }

    private sealed class CollectionUpdateHandle : global::System.IDisposable
    {
        private readonly BatchObservableCollection<T> _collection;

        private int _isDisposed;

        internal CollectionUpdateHandle(in BatchObservableCollection<T> collection)
        {
            _collection = collection;
        }

        public void Dispose()
        {
            if (global::System.Threading.Interlocked.Exchange(ref _isDisposed, 1) == 0)
            {
                _collection.EndUpdate();
            }
        }
    }
}
