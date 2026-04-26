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
