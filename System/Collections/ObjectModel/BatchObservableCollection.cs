using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Codify.System.Collections.ObjectModel;

public class BatchObservableCollection<T> : ObservableCollection<T>
{
    private readonly object _collectionUpdateLock = new();

    private readonly PropertyChangedEventArgs _countPropertyChangedEventArgs = new(nameof(Count));

    private bool _isUpdating;

    public IDisposable BeginUpdate()
    {
        lock (_collectionUpdateLock)
        {
            _isUpdating = true;
        }

        return new CollectionUpdateHandle(this);
    }

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

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        lock (_collectionUpdateLock)
        {
            if (_isUpdating) return;
        }

        NotifyCollectionChanged(e);
    }

    private class CollectionUpdateHandle : IDisposable
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