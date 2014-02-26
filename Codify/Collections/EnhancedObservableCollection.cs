using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace Codify.Windows.Collections
{
    /// <summary>
    ///     This collection automatically raises the <see cref="ObservableCollection{T}.CollectionChanged" /> event in the UI
    ///     thread. It also provides the <see cref="BeginUpdate" /> and <see cref="EndUpdate" /> methods for better
    ///     performance when dealing with multiple collection updates at once. However, if these methods are not used when
    ///     dealing large set of changes, this collection will have dramatically slower performance than a normal
    ///     <see cref="ObservableCollection{T}" />.
    /// </summary>
    [Serializable]
    public class EnhancedObservableCollection<T> : ObservableCollection<T>
    {
        private const string CountPropertyName = "Count";
        private const string IndexerName = Binding.IndexerName;
        private readonly Dispatcher _applicationDispatcher = Application.Current.Dispatcher;
        private uint _count = uint.MinValue;

        private readonly object _lockObject = new object();

        public void BeginUpdate()
        {
            lock (_lockObject)
                _count++;
        }

        public void EndUpdate()
        {
            lock (_lockObject)
            {
                if (_count == uint.MinValue)
                    return;
                if (--_count > uint.MinValue) return;
            }
            OnPropertyChanged(new PropertyChangedEventArgs(CountPropertyName));
            OnPropertyChanged(new PropertyChangedEventArgs(IndexerName));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            lock (_lockObject)
                if (_count > uint.MinValue)
                    return;
            if (_applicationDispatcher.CheckAccess())
                base.OnCollectionChanged(e);
            else
                _applicationDispatcher.Invoke((Action)(() => base.OnCollectionChanged(e)));
        }
    }
}