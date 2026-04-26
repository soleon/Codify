using Codify.System.Windows.Input;

namespace Codify.System.Windows.Tests.Input;

public class ActionCommandTests
{
    [Fact]
    public void NonGenericCommandExecutesWithNullParameter()
    {
        var executed = false;
        var command = new ActionCommand(() => executed = true);

        Assert.True(command.CanExecute(null!));
        command.Execute(null!);

        Assert.True(executed);
    }

    [Fact]
    public void ConstructorRejectsNullExecuteAction()
    {
        Assert.Throws<ArgumentNullException>(() => new ActionCommand(null!));
        Assert.Throws<ArgumentNullException>(() => new ActionCommand<int>(null!));
    }

    [Fact]
    public void NotifyCanExecuteChangedRaisesEventWithCommandSender()
    {
        var command = new ActionCommand(() => { });
        object? sender = null;
        EventArgs? eventArgs = null;
        var eventCount = 0;
        command.CanExecuteChanged += (s, e) =>
        {
            sender = s;
            eventArgs = e;
            eventCount++;
        };

        command.NotifyCanExecuteChanged();

        Assert.Equal(1, eventCount);
        Assert.Same(command, sender);
        Assert.Same(EventArgs.Empty, eventArgs);
    }

    [Fact]
    public void NonGenericCommandDoesNotExecuteWhenDisabled()
    {
        var executed = false;
        var command = new ActionCommand(() => executed = true, () => false);

        Assert.False(command.CanExecute(null!));
        command.Execute(null!);

        Assert.False(executed);
    }

    [Fact]
    public void GenericValueTypeCommandRejectsNullParameterWithoutExecuting()
    {
        var executed = false;
        var command = new ActionCommand<int>(_ => executed = true);

        Assert.False(command.CanExecute(null!));
        command.Execute(null!);

        Assert.False(executed);
    }

    [Fact]
    public void GenericReferenceTypeCommandPassesNullParameterWhenEnabled()
    {
        string? received = "initial";
        var command = new ActionCommand<string>(value => received = value, value => value is null);

        Assert.True(command.CanExecute(null!));
        command.Execute(null!);

        Assert.Null(received);
    }

    [Fact]
    public void GenericReferenceTypeCommandDoesNotExecuteNullParameterWhenDisabled()
    {
        var executed = false;
        var command = new ActionCommand<string>(_ => executed = true, value => value is not null);

        Assert.False(command.CanExecute(null!));
        command.Execute(null!);

        Assert.False(executed);
    }

    [Fact]
    public void GenericCommandPassesValidParameterToPredicateAndExecuteAction()
    {
        var receivedByPredicate = 0;
        var receivedByExecute = 0;
        var command = new ActionCommand<int>(
            value => receivedByExecute = value,
            value =>
            {
                receivedByPredicate = value;
                return true;
            });

        Assert.True(command.CanExecute(42));
        command.Execute(42);

        Assert.Equal(42, receivedByPredicate);
        Assert.Equal(42, receivedByExecute);
    }

    [Fact]
    public void GenericCommandReportsInvalidParameterTypeConsistently()
    {
        var command = new ActionCommand<int>(_ => { });
        const string expectedMessage =
            "System.String is not a valid parameter type for this command. Expected System.Int32.";

        var canExecuteException = Assert.Throws<InvalidOperationException>(() => command.CanExecute("bad"));
        var executeException = Assert.Throws<InvalidOperationException>(() => command.Execute("bad"));

        Assert.Equal(expectedMessage, canExecuteException.Message);
        Assert.Equal(canExecuteException.Message, executeException.Message);
    }

    [Fact]
    public void GenericCommandDoesNotExecuteValidParameterWhenDisabled()
    {
        var executed = false;
        var command = new ActionCommand<int>(_ => executed = true, _ => false);

        Assert.False(command.CanExecute(42));
        command.Execute(42);

        Assert.False(executed);
    }
}
