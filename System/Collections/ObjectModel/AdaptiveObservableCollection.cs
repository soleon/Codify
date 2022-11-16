using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Codify.System.Collections.ObjectModel
{
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
        ///     The function used to convert <see cref="TSource" /> items to <see cref="TTarget" /> items.
        /// </param>
        public AdaptiveObservableCollection(ObservableCollection<TSource> sourceCollection, Func<TSource, TTarget> converter)
        {
            sourceCollection.CollectionChanged += OnSourceCollectionChanged;
            _sourceCollection = sourceCollection;
            _converter = converter;

            Reload();
        }

        /// <summary>
        ///     Clears existing items and stops synchronising with the source collection.
        /// </summary>
        public void Dispose()
        {
            _sourceCollection.CollectionChanged -= OnSourceCollectionChanged;
            ClearItems();
        }

        private void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var newIndex = e.NewStartingIndex;
                    foreach (TSource item in e.NewItems)
                    {
                        InsertItem(newIndex >= Count ? Count : newIndex, _converter(item));
                    }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    var oldIndex = e.OldStartingIndex;
                    foreach (var _ in e.OldItems)
                    {
                        RemoveItem(oldIndex++);
                    }

                    break;
                case NotifyCollectionChangedAction.Replace:
                    SetItem(e.NewStartingIndex, _converter((TSource)e.NewItems[0]));
                    break;
                case NotifyCollectionChangedAction.Reset:
                    ClearItems();
                    Reload();

                    break;
                case NotifyCollectionChangedAction.Move:
                    MoveItem(e.OldStartingIndex, e.NewStartingIndex);

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Reload()
        {
            foreach (var targetItem in _sourceCollection.Select(_converter))
            {
                Add(targetItem);
            }
        }
    }
}