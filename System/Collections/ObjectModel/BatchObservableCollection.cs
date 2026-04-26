using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Codify.System.Collections.ObjectModel;

/// <summary>
/// Provides an observable collection that can suppress intermediate notifications during a batch update.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
public class BatchObservableCollection<T> : ObservableCollection<T>
{
    private readonly global::System.Threading.Lock _collectionUpdateLock = new();

    private readonly PropertyChangedEventArgs _countPropertyChangedEventArgs = new(nameof(Count));

    private bool _isUpdating;

    /// <summary>
    /// Begins a batch update and returns a handle that ends the update when disposed.
    /// </summary>
    /// <returns>
    /// A disposable handle that raises a reset notification after the update completes.
    /// </returns>
    public IDisposable BeginUpdate()
    {
        lock (_collectionUpdateLock)
        {
            _isUpdating = true;
        }

        return new CollectionUpdateHandle(this);
    }

    /// <summary>
    /// Raises a property change notification unless a batch update is active.
    /// </summary>
    /// <param name="e">The property change data.</param>
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (!_isUpdating) base.OnPropertyChanged(e);
    }

    private void NotifyCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnCollectionChanged(e);
    }

    private void EndUpdate()
    {
        lock (_collectionUpdateLock)
        {
            _isUpdating = false;
        }

        NotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        OnPropertyChanged(_countPropertyChangedEventArgs);
    }

    /// <summary>
    /// Raises a collection change notification unless a batch update is active.
    /// </summary>
    /// <param name="e">The collection change data.</param>
    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        lock (_collectionUpdateLock)
        {
            if (_isUpdating) return;
        }

        NotifyCollectionChanged(e);
    }

    private sealed class CollectionUpdateHandle : IDisposable
    {
        private readonly BatchObservableCollection<T> _collection;

        internal CollectionUpdateHandle(in BatchObservableCollection<T> collection)
        {
            _collection = collection;
        }

        public void Dispose()
        {
            _collection.EndUpdate();
        }
    }
}
