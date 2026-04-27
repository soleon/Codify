using Codify.System.Windows.Input;

namespace Codify.System.Windows.Tests.Input;

public class CommandTests
{
    [Fact]
    public void CanExecuteReturnsTrueWhenCanExecuteFuncIsNull()
    {
        var command = new TestCommand();

        Assert.True(command.CanExecute(null));
        Assert.True(command.CanExecute("anything"));
    }

    [Fact]
    public void NotifyCanExecuteChangedRaisesCanExecuteChangedWithSenderAndEmptyArgs()
    {
        var command = new TestCommand();
        object? capturedSender = null;
        EventArgs? capturedArgs = null;
        command.CanExecuteChanged += (sender, args) =>
        {
            capturedSender = sender;
            capturedArgs = args;
        };

        command.NotifyCanExecuteChanged();

        Assert.Same(command, capturedSender);
        Assert.Same(EventArgs.Empty, capturedArgs);
    }

    [Fact]
    public void NotifyCanExecuteChangedIsSafeWithNoSubscribers()
    {
        var command = new TestCommand();

        command.NotifyCanExecuteChanged();
    }

    private sealed class TestCommand : Command
    {
        public override void Execute(object? parameter)
        {
        }
    }
}
