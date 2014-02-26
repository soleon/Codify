using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Codify.Extensions;

namespace Codify.Windows.Collections
{
    /// <summary>
    ///     A read only observable collection that synchronises to a source collection of a different item type.
    /// </summary>
    /// <typeparam name="TSource"> The type of the items in the source collection. </typeparam>
    /// <typeparam name="TTarget"> The type of the items in this collection. </typeparam>
    public class AdaptedObservableCollection<TSource, TTarget> : ObservableCollection<TTarget>, IDisposable
    {
        #region Private Members

        /// <summary>
        ///     A reference to the function used to wrap source items.
        /// </summary>
        private readonly Func<TSource, TTarget> _converter;

        /// <summary>
        ///     An internal list that keep track of the exact source items that maps 1 to 1 to target items in this collection.
        /// </summary>
        private readonly ConditionalObservableCollection<TSource> _conditionalSourceCollection;

        #endregion


        #region Constructors

        /// <summary>
        ///     Creates a new instance of <see cref="AdaptedObservableCollection{TSource, TTarget}" /> .
        /// </summary>
        /// <param name="converter">
        ///     The function used to convert <see cref="TSource" /> items to <see cref="TTarget" /> items.
        /// </param>
        /// <param name="source">
        ///     The observable collection of <see cref="TSource" /> items.
        /// </param>
        /// <param name="filter">
        ///     The function used to filter <see cref="TSource" /> items.
        /// </param>
        /// <param name="comparer"> The comparer function to determine the order of the items in this collection. This function compares two specified TSource objects and returns an integer that indicates their relationship to one another in the sort order. Return less than 0 for object A is less than object B, 0 for object A equals to object B, greater than 0 for object A is greater than object B. </param>
        /// <param name="maxItemsCount"> The maximum number of items allowed in this collection. </param>
        public AdaptedObservableCollection(Func<TSource, TTarget> converter, Func<TSource, bool> filter = null, Func<TSource, TSource, int> comparer = null, INotifyCollectionChanged source = null, int maxItemsCount = 0)
        {
            _conditionalSourceCollection = new ConditionalObservableCollection<TSource>(filter, comparer, source, maxItemsCount);
            _conditionalSourceCollection.CollectionChanged += OnSourceCollectionChanged;
            _converter = converter;

            // Initial sync.
            _conditionalSourceCollection.Select(converter).ForEach(Add);
        }

        #endregion


        #region Public Properties

        /// <summary>
        ///     Gets or sets the source collection to bind to.
        /// </summary>
        public INotifyCollectionChanged Source
        {
            get { return _conditionalSourceCollection.Source; }
            set { _conditionalSourceCollection.Source = value; }
        }

        /// <summary>
        ///     Clears existing items and stops synchronising with the current <see cref="Source" /> collection.
        /// </summary>
        public void Dispose()
        {
            _conditionalSourceCollection.Dispose();
        }

        #endregion


        #region Public Methods

        /// <summary>
        ///     Forces a refresh of this collection.
        /// </summary>
        public void Refresh()
        {
            _conditionalSourceCollection.Refresh();
        }

        #endregion


        #region Event Handling

        private void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    var index = e.NewStartingIndex;
                    foreach (TSource item in e.NewItems)
                        this.SafeInsert(index++, _converter(item));
                    break;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    var index = e.OldStartingIndex;
                    foreach (var item in e.OldItems) RemoveItem(index++);
                    break;
                }
                case NotifyCollectionChangedAction.Replace:
                    SetItem(e.NewStartingIndex, _converter((TSource)e.NewItems[0]));
                    break;
                case NotifyCollectionChangedAction.Reset:
                    ClearItems();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion


        #region Protected Methods

        // Note: These methods are overriden with protected access modifier to limit direct modification of this collection from external sources.
        // However, this does not prevent direct modification if this class is casted to one of its base types.

        protected new void Add(TTarget item)
        {
            base.Add(item);
        }

        protected new void Remove(TTarget item)
        {
            base.Remove(item);
        }

        protected new void Insert(int index, TTarget item)
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
    }
}