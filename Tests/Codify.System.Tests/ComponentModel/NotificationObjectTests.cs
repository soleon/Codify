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
    public void SetValueUsesGenericEqualityForValueTypes()
    {
        EquatableValue.ObjectEqualsCallCount = 0;
        var target = new TestNotificationObject();
        var propertyNames = TrackPropertyChanges(target);

        var changed = target.SetEquatableValue(new EquatableValue(1));

        Assert.False(changed);
        Assert.Equal(0, EquatableValue.ObjectEqualsCallCount);
        Assert.Empty(propertyNames);
    }

    [Fact]
    public void SetValueUsesGenericEqualityForReferenceTypes()
    {
        EquatableReference.ObjectEqualsCallCount = 0;
        var target = new TestNotificationObject();
        var propertyNames = TrackPropertyChanges(target);

        var changed = target.SetEquatableReference(new EquatableReference(1));

        Assert.False(changed);
        Assert.Equal(0, EquatableReference.ObjectEqualsCallCount);
        Assert.Empty(propertyNames);
    }

    [Fact]
    public void SetValueUsesGenericEqualityForNullReferenceValues()
    {
        EquatableReference.ObjectEqualsCallCount = 0;
        var target = new TestNotificationObject();
        var propertyNames = TrackPropertyChanges(target);

        var changed = target.SetNullableEquatableReference(null);

        Assert.True(changed);
        Assert.Null(target.NullableEquatableReference);
        Assert.Equal(["NullableEquatableReference"], propertyNames);
        Assert.Equal(0, EquatableReference.ObjectEqualsCallCount);

        propertyNames.Clear();

        changed = target.SetNullableEquatableReference(null);

        Assert.False(changed);
        Assert.Equal(0, EquatableReference.ObjectEqualsCallCount);
        Assert.Empty(propertyNames);
    }

    [Fact]
    public void RepeatedPropertyChangesReuseEventArgsByPropertyName()
    {
        var target = new TestNotificationObject();
        var args = TrackPropertyChangeArgs(target);

        target.SetPrimary("first");
        target.SetPrimary("second");

        Assert.Collection(
            args,
            arg => Assert.Equal("Primary", arg.PropertyName),
            arg => Assert.Equal("Dependent", arg.PropertyName),
            arg => Assert.Equal("Primary", arg.PropertyName),
            arg => Assert.Equal("Dependent", arg.PropertyName));
        Assert.Same(args[0], args[2]);
        Assert.Same(args[1], args[3]);
        Assert.NotSame(args[0], args[1]);
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

    private static List<global::System.ComponentModel.PropertyChangedEventArgs> TrackPropertyChangeArgs(
        NotificationObject target)
    {
        var args = new List<global::System.ComponentModel.PropertyChangedEventArgs>();
        target.PropertyChanged += (_, eventArgs) => args.Add(eventArgs);
        return args;
    }

    private readonly struct EquatableValue : IEquatable<EquatableValue>
    {
        public EquatableValue(int value)
        {
            Value = value;
        }

        public static int ObjectEqualsCallCount { get; set; }

        private int Value { get; }

        public bool Equals(EquatableValue other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object? obj)
        {
            ObjectEqualsCallCount++;
            return obj is EquatableValue other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value;
        }
    }

    private sealed class EquatableReference : IEquatable<EquatableReference>
    {
        public EquatableReference(int value)
        {
            Value = value;
        }

        public static int ObjectEqualsCallCount { get; set; }

        private int Value { get; }

        public bool Equals(EquatableReference? other)
        {
            return other is not null && Value == other.Value;
        }

        public override bool Equals(object? obj)
        {
            ObjectEqualsCallCount++;
            return obj is EquatableReference other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value;
        }
    }

    private sealed class TestNotificationObject : NotificationObject
    {
        private string _callerNamed = "initial";
        private EquatableReference _equatableReference = new(1);
        private EquatableValue _equatableValue = new(1);
        private EquatableReference? _nullableEquatableReference = new(1);
        private string _primary = "initial";

        public string CallerNamed
        {
            get => _callerNamed;
            set => SetValue(ref _callerNamed, value);
        }

        public string Dependent => _primary.ToUpperInvariant();

        public EquatableReference? NullableEquatableReference => _nullableEquatableReference;

        public string Primary => _primary;

        public bool SetEquatableReference(EquatableReference value)
        {
            return SetValue(ref _equatableReference, value, nameof(SetEquatableReference));
        }

        public bool SetEquatableValue(EquatableValue value)
        {
            return SetValue(ref _equatableValue, value, nameof(SetEquatableValue));
        }

        public bool SetNullableEquatableReference(EquatableReference? value)
        {
            return SetValue(
                ref _nullableEquatableReference,
                value,
                nameof(NullableEquatableReference));
        }

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
