using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Codify.System.Collections.ObjectModel;

/// <summary>
///     Provides an observable collection that can defer intermediate change notifications during update scopes.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
public class DeferrableObservableCollection<T> : ObservableCollection<T>
{
    private static readonly PropertyChangedEventArgs CountPropertyChangedEventArgs = new(nameof(Count));

    private static readonly PropertyChangedEventArgs IndexerPropertyChangedEventArgs = new("Item[]");

    private static readonly NotifyCollectionChangedEventArgs ResetCollectionChangedEventArgs =
        new(NotifyCollectionChangedAction.Reset);

    private readonly Lock _collectionUpdateLock = new();

    private bool _hasPendingChanges;

    private int _updateDepth;

    /// <summary>
    ///     Begins an update scope and returns a handle that ends the scope when disposed.
    /// </summary>
    /// <returns>
    ///     A disposable handle that raises a reset notification after the update scope completes.
    /// </returns>
    public IDisposable BeginUpdate()
    {
        lock (_collectionUpdateLock)
        {
            _updateDepth++;
        }

        return new CollectionUpdateHandle(this);
    }

    private void EndUpdate()
    {
        bool shouldNotify;

        lock (_collectionUpdateLock)
        {
            _updateDepth--;
            shouldNotify = _updateDepth == 0 && _hasPendingChanges;
            if (_updateDepth == 0)
            {
                _hasPendingChanges = false;
            }
        }

        if (!shouldNotify)
        {
            return;
        }

        OnPropertyChanged(CountPropertyChangedEventArgs);
        OnPropertyChanged(IndexerPropertyChangedEventArgs);
        NotifyCollectionChanged(ResetCollectionChangedEventArgs);
    }

    private void NotifyCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnCollectionChanged(e);
    }

    /// <summary>
    ///     Raises a collection change notification unless an update scope is active.
    /// </summary>
    /// <param name="e">The collection change data.</param>
    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        lock (_collectionUpdateLock)
        {
            if (_updateDepth > 0)
            {
                _hasPendingChanges = true;
                return;
            }
        }

        NotifyCollectionChanged(e);
    }

    /// <summary>
    ///     Raises a property change notification unless an update scope is active.
    /// </summary>
    /// <param name="e">The property change data.</param>
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        lock (_collectionUpdateLock)
        {
            if (_updateDepth > 0)
            {
                _hasPendingChanges = true;
                return;
            }
        }

        base.OnPropertyChanged(e);
    }

    private sealed class CollectionUpdateHandle : IDisposable
    {
        private readonly DeferrableObservableCollection<T> _collection;

        private int _isDisposed;

        internal CollectionUpdateHandle(in DeferrableObservableCollection<T> collection)
        {
            _collection = collection;
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _isDisposed, 1) == 0)
            {
                _collection.EndUpdate();
            }
        }
    }
}
