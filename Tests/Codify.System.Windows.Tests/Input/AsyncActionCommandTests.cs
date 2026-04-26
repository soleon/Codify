using Codify.System.Windows.Input;

namespace Codify.System.Windows.Tests.Input;

public class AsyncActionCommandTests
{
    [Fact]
    public async Task NonGenericCommandExecutesWithNullParameter()
    {
        var executed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var command = new AsyncActionCommand(() =>
        {
            executed.SetResult();
            return Task.CompletedTask;
        });

        Assert.True(command.CanExecute(null!));
        command.Execute(null!);

        await executed.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }

    [Fact]
    public void NonGenericCommandDoesNotExecuteWhenDisabled()
    {
        var executed = false;
        var command = new AsyncActionCommand(() =>
        {
            executed = true;
            return Task.CompletedTask;
        }, () => false);

        Assert.False(command.CanExecute(null!));
        command.Execute(null!);

        Assert.False(executed);
    }

    [Fact]
    public void GenericValueTypeCommandRejectsNullParameter()
    {
        var executed = false;
        var command = new AsyncActionCommand<int>(_ =>
        {
            executed = true;
            return Task.CompletedTask;
        });

        Assert.False(command.CanExecute(null!));
        command.Execute(null!);

        Assert.False(executed);
    }

    [Fact]
    public async Task GenericReferenceTypeCommandPassesNullParameterWhenEnabled()
    {
        var executed = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var command = new AsyncActionCommand<string>(value =>
        {
            executed.SetResult(value);
            return Task.CompletedTask;
        }, value => value is null);

        Assert.True(command.CanExecute(null!));
        command.Execute(null!);

        Assert.Null(await executed.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));
    }

    [Fact]
    public void GenericReferenceTypeCommandDoesNotExecuteNullParameterWhenDisabled()
    {
        var executed = false;
        var command = new AsyncActionCommand<string>(_ =>
        {
            executed = true;
            return Task.CompletedTask;
        }, value => value is not null);

        Assert.False(command.CanExecute(null!));
        command.Execute(null!);

        Assert.False(executed);
    }

    [Fact]
    public void GenericCommandReportsInvalidParameterTypeConsistently()
    {
        var command = new AsyncActionCommand<int>(_ => Task.CompletedTask);
        const string expectedMessage =
            "System.String is not a valid parameter type for this command. Expected System.Int32.";

        var canExecuteException = Assert.Throws<InvalidOperationException>(() => command.CanExecute("bad"));
        var executeException = CaptureAsyncVoidException(() => command.Execute("bad"));

        Assert.Equal(expectedMessage, canExecuteException.Message);
        Assert.Equal(canExecuteException.Message, executeException.Message);
    }

    [Fact]
    public async Task CommandExecuteReportsAsyncExceptionsToSynchronizationContext()
    {
        var expectedException = new InvalidOperationException("async failure");
        var exception = await CaptureAsyncVoidExceptionAsync(
            () => new AsyncActionCommand(() => Task.FromException(expectedException)).Execute(null!));

        Assert.Same(expectedException, exception);
    }

    private static Exception CaptureAsyncVoidException(Action execute)
    {
        return CaptureAsyncVoidExceptionAsync(execute).GetAwaiter().GetResult();
    }

    private static async Task<Exception> CaptureAsyncVoidExceptionAsync(Action execute)
    {
        var originalContext = SynchronizationContext.Current;
        var context = new RecordingSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(context);

        try
        {
            execute();
            return await context.ExceptionTask.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(originalContext);
        }
    }

    private sealed class RecordingSynchronizationContext : SynchronizationContext
    {
        private readonly TaskCompletionSource<Exception> _exception =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task<Exception> ExceptionTask => _exception.Task;

        public override void Post(SendOrPostCallback callback, object? state)
        {
            try
            {
                callback(state);
            }
            catch (Exception exception)
            {
                _exception.TrySetResult(exception);
            }
        }
    }
}
