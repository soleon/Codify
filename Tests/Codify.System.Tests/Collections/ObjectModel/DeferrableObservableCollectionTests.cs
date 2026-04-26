using System.Collections.Specialized;
using System.ComponentModel;
using Codify.System.Collections.ObjectModel;

namespace Codify.System.Tests.Collections.ObjectModel;

public class DeferrableObservableCollectionTests
{
    [Fact]
    public void CollectionChangesOutsideUpdateRaiseNormalNotifications()
    {
        var collection = new DeferrableObservableCollection<int>();
        var collectionEvents = TrackCollectionChanges(collection);
        var propertyNames = TrackPropertyChanges(collection);

        collection.Add(42);

        var collectionEvent = Assert.Single(collectionEvents);
        Assert.Equal(NotifyCollectionChangedAction.Add, collectionEvent.Action);
        Assert.Equal(0, collectionEvent.NewStartingIndex);
        Assert.Equal([42], collectionEvent.NewItems!.Cast<int>());
        Assert.Equal(["Count", "Item[]"], propertyNames);
    }

    [Fact]
    public void BeginUpdateSuppressesIntermediateNotificationsUntilDisposed()
    {
        var collection = new DeferrableObservableCollection<int> { 1 };
        var collectionEvents = TrackCollectionChanges(collection);
        var propertyNames = TrackPropertyChanges(collection);

        using (collection.BeginUpdate())
        {
            collection.Add(2);
            collection.Remove(1);

            Assert.Empty(collectionEvents);
            Assert.Empty(propertyNames);
        }

        var collectionEvent = Assert.Single(collectionEvents);
        Assert.Equal(NotifyCollectionChangedAction.Reset, collectionEvent.Action);
        Assert.Equal(["Count", "Item[]"], propertyNames);
        Assert.Equal([2], collection);
    }

    [Fact]
    public void CompletedUpdateWithoutChangesDoesNotRaiseResetNotification()
    {
        var collection = new DeferrableObservableCollection<int> { 1 };
        var collectionEvents = TrackCollectionChanges(collection);
        var propertyNames = TrackPropertyChanges(collection);

        using (collection.BeginUpdate())
        {
        }

        Assert.Empty(collectionEvents);
        Assert.Empty(propertyNames);
        Assert.Equal([1], collection);
    }

    [Fact]
    public void CompletedUpdateRaisesIndexerPropertyNotification()
    {
        var collection = new DeferrableObservableCollection<int> { 1 };
        var propertyNames = TrackPropertyChanges(collection);

        using (collection.BeginUpdate())
        {
            collection[0] = 2;
        }

        Assert.Contains("Item[]", propertyNames);
    }

    [Fact]
    public void BeginUpdateReturnsHandleThatCanBeDisposedExplicitly()
    {
        var collection = new DeferrableObservableCollection<int>();
        var collectionEvents = TrackCollectionChanges(collection);

        var update = collection.BeginUpdate();
        collection.Add(1);
        update.Dispose();

        var collectionEvent = Assert.Single(collectionEvents);
        Assert.Equal(NotifyCollectionChangedAction.Reset, collectionEvent.Action);
        Assert.Equal([1], collection);
    }

    [Fact]
    public void NestedUpdatesSuppressNotificationsUntilOutermostHandleIsDisposed()
    {
        var collection = new DeferrableObservableCollection<int> { 1 };
        var collectionEvents = TrackCollectionChanges(collection);
        var propertyNames = TrackPropertyChanges(collection);

        using (var outerUpdate = collection.BeginUpdate())
        {
            collection.Add(2);

            using (collection.BeginUpdate())
            {
                collection.Add(3);
            }

            Assert.Empty(collectionEvents);
            Assert.Empty(propertyNames);

            outerUpdate.Dispose();
        }

        var collectionEvent = Assert.Single(collectionEvents);
        Assert.Equal(NotifyCollectionChangedAction.Reset, collectionEvent.Action);
        Assert.Equal(["Count", "Item[]"], propertyNames);
        Assert.Equal([1, 2, 3], collection);
    }

    [Fact]
    public void DisposingUpdateHandleMoreThanOnceRaisesOneResetNotification()
    {
        var collection = new DeferrableObservableCollection<int>();
        var collectionEvents = TrackCollectionChanges(collection);
        var propertyNames = TrackPropertyChanges(collection);

        var update = collection.BeginUpdate();
        collection.Add(1);

        update.Dispose();
        update.Dispose();

        Assert.Single(collectionEvents);
        Assert.Equal(["Count", "Item[]"], propertyNames);
        Assert.Equal([1], collection);
    }

    private static List<NotifyCollectionChangedEventArgs> TrackCollectionChanges<T>(
        DeferrableObservableCollection<T> collection)
    {
        var events = new List<NotifyCollectionChangedEventArgs>();
        collection.CollectionChanged += (_, args) => events.Add(args);
        return events;
    }

    private static List<string?> TrackPropertyChanges<T>(DeferrableObservableCollection<T> collection)
    {
        var propertyNames = new List<string?>();
        ((INotifyPropertyChanged)collection).PropertyChanged += (_, args) => propertyNames.Add(args.PropertyName);
        return propertyNames;
    }
}
