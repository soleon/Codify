using Codify.System.ComponentModel;

namespace Codify.System.Tests.ComponentModel;

public class ExpandableNotificationObjectTests
{
    [Fact]
    public void IsExpandedChangeRaisesPropertyChangedThenSynchronousHookThenAsyncHook()
    {
        var target = new TrackingExpandableNotificationObject();
        target.PropertyChanged += (_, args) => target.Events.Add($"PropertyChanged:{args.PropertyName}");

        target.IsExpanded = true;

        Assert.Equal(
        [
            "PropertyChanged:IsExpanded",
            "ExpansionChanged:True",
            "ExpansionChangedAsync:True"
        ], target.Events);
    }

    [Fact]
    public void IsSelectedChangeRaisesPropertyChangedThenSynchronousHookThenAsyncHook()
    {
        var target = new TrackingExpandableNotificationObject();
        target.PropertyChanged += (_, args) => target.Events.Add($"PropertyChanged:{args.PropertyName}");

        target.IsSelected = true;

        Assert.Equal(
        [
            "PropertyChanged:IsSelected",
            "SelectionChanged:True",
            "SelectionChangedAsync:True"
        ], target.Events);
    }

    [Fact]
    public void UnchangedValuesDoNotRunSynchronousOrAsyncHooks()
    {
        var target = new TrackingExpandableNotificationObject
        {
            IsExpanded = true,
            IsSelected = true
        };
        target.Events.Clear();

        target.IsExpanded = true;
        target.IsSelected = true;

        Assert.Empty(target.Events);
    }

    [Fact]
    public async Task FaultedExpansionAsyncHookIsObservedWithoutThrowingFromSetter()
    {
        var expectedException = new InvalidOperationException("Expansion failed.");
        var target = FaultingExpandableNotificationObject.ForExpansion(expectedException);

        var setterException = Record.Exception(() => target.IsExpanded = true);
        var observedException = await target.ObservedException.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Null(setterException);
        Assert.Same(expectedException, observedException);
    }

    [Fact]
    public async Task FaultedSelectionAsyncHookIsObservedWithoutThrowingFromSetter()
    {
        var expectedException = new InvalidOperationException("Selection failed.");
        var target = FaultingExpandableNotificationObject.ForSelection(expectedException);

        var setterException = Record.Exception(() => target.IsSelected = true);
        var observedException = await target.ObservedException.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Null(setterException);
        Assert.Same(expectedException, observedException);
    }

    private sealed class TrackingExpandableNotificationObject : ExpandableNotificationObject
    {
        public List<string> Events { get; } = [];

        protected override void OnExpansionChanged(bool isExpended)
        {
            Events.Add($"ExpansionChanged:{isExpended}");
        }

        protected override Task OnExpansionChangedAsync(bool isExpended)
        {
            Events.Add($"ExpansionChangedAsync:{isExpended}");
            return Task.CompletedTask;
        }

        protected override void OnSelectionChanged(bool isSelected)
        {
            Events.Add($"SelectionChanged:{isSelected}");
        }

        protected override Task OnSelectionChangedAsync(bool isSelected)
        {
            Events.Add($"SelectionChangedAsync:{isSelected}");
            return Task.CompletedTask;
        }
    }

    private sealed class FaultingExpandableNotificationObject : ExpandableNotificationObject
    {
        private readonly Exception _exception;
        private readonly bool _faultExpansion;
        private readonly TaskCompletionSource<Exception> _observedException =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        private FaultingExpandableNotificationObject(Exception exception, bool faultExpansion)
        {
            _exception = exception;
            _faultExpansion = faultExpansion;
        }

        public Task<Exception> ObservedException => _observedException.Task;

        public static FaultingExpandableNotificationObject ForExpansion(Exception exception)
        {
            return new FaultingExpandableNotificationObject(exception, faultExpansion: true);
        }

        public static FaultingExpandableNotificationObject ForSelection(Exception exception)
        {
            return new FaultingExpandableNotificationObject(exception, faultExpansion: false);
        }

        protected override Task OnExpansionChangedAsync(bool isExpended)
        {
            return _faultExpansion ? Task.FromException(_exception) : Task.CompletedTask;
        }

        protected override Task OnSelectionChangedAsync(bool isSelected)
        {
            return _faultExpansion ? Task.CompletedTask : Task.FromException(_exception);
        }

        protected override void OnAsyncHookException(Exception exception)
        {
            _observedException.TrySetResult(exception);
        }
    }
}
