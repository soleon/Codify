using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Codify.Extensions;

namespace Codify.Windows.Collections
{
    /// <summary>
    ///     A read only observable collection that conditionally synchronises to a source
    ///     <see cref="INotifyCollectionChanged" /> collection.
    /// </summary>
    /// <typeparam name="T"> The type of items in the collection. </typeparam>
    public class ConditionalObservableCollection<T> : ObservableCollection<T>, IDisposable
    {
        #region Private Fields

        /// <summary>
        ///     A reference to the function used to determine if the source item passes the condition.
        /// </summary>
        private readonly Func<T, bool> _filter;

        #endregion


        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConditionalObservableCollection{T}" /> class that conditionally
        ///     synchronises with the source collection.
        /// </summary>
        /// <param name="filter"> The filter function to determine which items to sync from the source collection. </param>
        /// <param name="comparer">
        ///     The comparer function to determine the order of the items in this collection. This function
        ///     compares two specified TSource objects and returns an integer that indicates their relationship to one another in
        ///     the sort order. Return less than 0 for object A is less than object B, 0 for object A equals to object B, greater
        ///     than 0 for object A is greater than object B.
        /// </param>
        /// <param name="source"> The source collection that this collection conditionally synchronises to. </param>
        /// <param name="maxItemsCount"> The maximum number of items allowed in this collection. </param>
        public ConditionalObservableCollection(Func<T, bool> filter = null, Func<T, T, int> comparer = null, INotifyCollectionChanged source = null, int maxItemsCount = 0)
        {
            _maxItemsCount = maxItemsCount;
            _filter = filter;
            _comparer = comparer;
            Source = source;
        }

        #endregion


        #region Public Properties

        /// <summary>
        ///     Gets or sets the source collection to bind to.
        /// </summary>
        public INotifyCollectionChanged Source
        {
            get { return _source; }
            set
            {
                if (_source == value) return;

                var newValue = value as IList<T>;

                if (value != null && newValue == null)
                    throw new InvalidOperationException("Source collection must be an observable IList<TSource>, such as ObservableCollection<TSource> or ReadOnlyObservableCollection<TSource>.");

                var oldValue = _source;
                _source = value;
                OnSourceChanged(oldValue, value);
            }
        }

        private INotifyCollectionChanged _source;

        /// <summary>
        ///     Gets or sets the function used to determine the order of the items in this collection.
        /// </summary>
        public Func<T, T, int> Comparer
        {
            get { return _comparer; }
            set
            {
                if (_comparer == value) return;
                _comparer = value;
                if (Count == 0) return;

                var items = this.ToArray();
                ClearItems();
                items.ForEach(i => this.ConditionalInsert(i, value));
            }
        }

        private Func<T, T, int> _comparer;

        /// <summary>
        ///     Gets or sets the maximum number of items allowed in this collection.
        /// </summary>
        public int MaxItemsCount
        {
            get { return _maxItemsCount; }
            set
            {
                if (_maxItemsCount == value) return;
                var sourceCollection = _source as IList;

                // Update this collection if there is a valid source collection.
                if (sourceCollection != null)
                {
                    // If the max item count is increased, only refresh the collection if the source collection has more items than this collection.
                    if ((_maxItemsCount < value || value == 0) && Count < sourceCollection.Count) OnSourceChanged(null, _source);
                    // If the max item count is decreased, just remove the extra items.
                    if (_maxItemsCount > value) for (var i = Count - 1; i > value - 1; i--) RemoveItem(i);
                }

                _maxItemsCount = value;
            }
        }

        private int _maxItemsCount;

        #endregion


        #region Public Methods

        /// <summary>
        ///     Forces a refresh of this collection.
        /// </summary>
        public void Refresh()
        {
            ClearItems();
            if (_source == null) return;
            OnSourceItemsAdded((IEnumerable) _source);
        }

        /// <summary>
        ///     Clears existing items and stops synchronising with the current <see cref="Source" /> collection.
        /// </summary>
        public void Dispose()
        {
            Source = null;
        }

        #endregion


        #region Private Methods

        /// <summary>
        ///     Determines if the given item meets the condition of this collection.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if filter passes or does not exist, otherwise <c>false</c> .
        /// </returns>
        private bool IsValidItem(T sourceItem)
        {
            return _filter == null || _filter(sourceItem);
        }

        #endregion


        #region Protected Methods

        // Note: These methods are overriden with protected access modifier to limit direct modification of this collection from external sources.
        // However, this does not prevent direct modification if this class is casted to one of its base types.

        protected new void Add(T item)
        {
            base.Add(item);
        }

        protected new void Remove(T item)
        {
            base.Remove(item);
        }

        protected new void Insert(int index, T item)
        {
            base.Insert(index, item);
        }

        protected new void RemoveAt(int index)
        {
            base.RemoveAt(index);
        }

        protected new void Clear()
        {
            base.Clear();
        }

        #endregion


        #region Event Handling

        /// <summary>
        ///     Called when the source is changed.
        /// </summary>
        /// <param name="oldSource"> The old source value. </param>
        /// <param name="newSource"> The new source value. </param>
        private void OnSourceChanged(INotifyCollectionChanged oldSource, INotifyCollectionChanged newSource)
        {
            // Unbind old source
            if (oldSource != null) oldSource.CollectionChanged -= OnSourceItemCollectionChanged;

            Refresh();

            // Bind new source collection
            newSource.CollectionChanged += OnSourceItemCollectionChanged;
        }

        /// <summary>
        ///     Called when the source items have changed
        /// </summary>
        /// <param name="sender"> </param>
        /// <param name="e"> </param>
        private void OnSourceItemCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    OnSourceItemsAdded(e.NewItems);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    OnSourceItemsRemoved(e.OldItems);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    OnSourceItemReplaced((T) e.OldItems[0], (T) e.NewItems[0]);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    Refresh();
                    break;
            }
        }

        /// <summary>
        ///     Called when a collection of items is added in the source collection.
        /// </summary>
        /// <param name="items"> The new items. </param>
        private void OnSourceItemsAdded(IEnumerable items)
        {
            // Go through each valid item in the added source items.
            foreach (var item in items.Cast<T>().Where(IsValidItem))
            {
                // If a comparer function is available, just let the comparer function to determine where to insert the new item.
                if (_comparer != null)
                {
                    this.ConditionalInsert(item, _comparer);
                    continue;
                }

                // If there is no comparer function specified, we need to determine the index of this new item in this collection.

                // Cast source collection to IList<TSource>, because the IndexOf method is required.
                var source = (IList<T>) _source;

                // Get the index of the source item in the source collection.
                var index = source.IndexOf(item);

                if (_filter != null && index > 0)
                {
                    // If there is a filter function or source item is not the first item in source collection,
                    // try to find the previous matching item in the internal source items cache.
                    var previousItem = default(T);
                    for (var i = index - 1; i >= 0; i--)
                    {
                        var currentItem = source[i];
                        if (!_filter(currentItem)) continue;
                        previousItem = currentItem;
                        break;
                    }

                    if (Equals(previousItem, default(T)))
                        // If no previous matching item can be found, then the new source item should be the first item in this collection.
                        index = 0;
                    else
                        // If a previous matching item is found, then the new source item should be the next item of this previous item.
                        index = IndexOf(previousItem) + 1;
                }

                this.SafeInsert(index, item);
            }

            // Trim last item until at max items limit
            while (_maxItemsCount > 0 && Count > _maxItemsCount)
                RemoveItem(Count - 1);
        }


        /// <summary>
        ///     Called when items are removed from the source collection.
        /// </summary>
        /// <param name="items"> The removed source items. </param>
        private void OnSourceItemsRemoved(IEnumerable items)
        {
            foreach (var index in from T sourceItem in items where IsValidItem(sourceItem) select IndexOf(sourceItem))
                RemoveItem(index);
        }

        /// <summary>
        ///     Called when an item is replaced in the source collection.
        /// </summary>
        /// <param name="oldItem"> The old source item. </param>
        /// <param name="newItem"> The new source item. </param>
        private void OnSourceItemReplaced(T oldItem, T newItem)
        {
            // Get the index of the old item first.
            var index = IndexOf(oldItem);

            if (IsValidItem(newItem))
                // If the new item is a valid item, try replace the old item with the new item.
                if (_comparer == null)
                    if (index == -1)
                        Add(newItem);
                    else
                        SetItem(index, newItem);
                else
                {
                    var newIndex = this.ConditionalIndex(newItem, _comparer);
                    if (newIndex == index) SetItem(index, newItem);
                    else
                    {
                        this.SafeRemoveAt(index);
                        this.SafeInsert(newIndex, newItem);
                    }
                }
            else
            {
                // If the new item is not a valid item, just remove the old item.
                this.SafeRemoveAt(index);
            }
        }

        #endregion
    }
}