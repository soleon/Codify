using Codify.System.ComponentModel;

namespace Codify.System.Tests.ComponentModel;

public class NotificationObjectTests
{
    [Fact]
    public void SetValueRaisesPropertyChangedForChangedValueAndDependentPropertiesInOrder()
    {
        var target = new TestNotificationObject();
        var propertyNames = TrackPropertyChanges(target);

        var changed = target.SetPrimary("updated");

        Assert.True(changed);
        Assert.Equal("updated", target.Primary);
        Assert.Equal(["Primary", "Dependent"], propertyNames);
    }

    [Fact]
    public void SetValueDoesNotRaisePropertyChangedWhenValueIsEqual()
    {
        var target = new TestNotificationObject();
        target.SetPrimary("updated");
        var propertyNames = TrackPropertyChanges(target);

        var changed = target.SetPrimary("updated");

        Assert.False(changed);
        Assert.Equal("updated", target.Primary);
        Assert.Empty(propertyNames);
    }

    [Fact]
    public void CallerMemberNameOverloadRaisesChangedPropertyName()
    {
        var target = new TestNotificationObject();
        var propertyNames = TrackPropertyChanges(target);

        target.CallerNamed = "updated";

        Assert.Equal(["CallerNamed"], propertyNames);
    }

    [Fact]
    public void NullDependentPropertyListRaisesOnlyPrimaryProperty()
    {
        var target = new TestNotificationObject();
        var propertyNames = TrackPropertyChanges(target);

        var changed = target.SetPrimaryWithNullDependents("updated");

        Assert.True(changed);
        Assert.Equal(["Primary"], propertyNames);
    }

    private static List<string?> TrackPropertyChanges(NotificationObject target)
    {
        var propertyNames = new List<string?>();
        target.PropertyChanged += (_, args) => propertyNames.Add(args.PropertyName);
        return propertyNames;
    }

    private sealed class TestNotificationObject : NotificationObject
    {
        private string _callerNamed = "initial";
        private string _primary = "initial";

        public string CallerNamed
        {
            get => _callerNamed;
            set => SetValue(ref _callerNamed, value);
        }

        public string Dependent => _primary.ToUpperInvariant();

        public string Primary => _primary;

        public bool SetPrimary(string value)
        {
            return SetValue(ref _primary, value, nameof(Primary), nameof(Dependent));
        }

        public bool SetPrimaryWithNullDependents(string value)
        {
            return SetValue(ref _primary, value, nameof(Primary), null!);
        }
    }
}
