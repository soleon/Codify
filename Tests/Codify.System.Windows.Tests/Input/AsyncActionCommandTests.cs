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
    public void ConstructorRejectsNullExecuteFunc()
    {
        Assert.Throws<ArgumentNullException>(() => new AsyncActionCommand(null!));
        Assert.Throws<ArgumentNullException>(() => new AsyncActionCommand<int>(null!));
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
    public void NonGenericCommandEvaluatesCanExecuteOnceWhenExecuting()
    {
        var canExecuteCalls = 0;
        var executed = false;
        var command = new AsyncActionCommand(() =>
        {
            executed = true;
            return Task.CompletedTask;
        }, () => ++canExecuteCalls == 1);

        command.Execute(null!);

        Assert.True(executed);
        Assert.Equal(1, canExecuteCalls);
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
    public async Task GenericCommandPassesValidParameterToPredicateAndExecuteFunc()
    {
        var receivedByPredicate = 0;
        var executed = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        var command = new AsyncActionCommand<int>(
            value =>
            {
                executed.SetResult(value);
                return Task.CompletedTask;
            },
            value =>
            {
                receivedByPredicate = value;
                return true;
            });

        Assert.True(command.CanExecute(42));
        command.Execute(42);

        Assert.Equal(42, receivedByPredicate);
        Assert.Equal(42, await executed.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));
    }

    [Fact]
    public void GenericCommandEvaluatesCanExecuteOnceWhenExecuting()
    {
        var canExecuteCalls = 0;
        var receivedByExecute = 0;
        var command = new AsyncActionCommand<int>(
            value =>
            {
                receivedByExecute = value;
                return Task.CompletedTask;
            },
            _ => ++canExecuteCalls == 1);

        command.Execute(42);

        Assert.Equal(42, receivedByExecute);
        Assert.Equal(1, canExecuteCalls);
    }

    [Fact]
    public void GenericCommandDoesNotExecuteValidParameterWhenDisabled()
    {
        var executed = false;
        var command = new AsyncActionCommand<int>(_ =>
        {
            executed = true;
            return Task.CompletedTask;
        }, _ => false);

        Assert.False(command.CanExecute(42));
        command.Execute(42);

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

    [Fact]
    public async Task ExecuteInvokesOnExecuteExceptionWhenDelegateThrows()
    {
        var expected = new InvalidOperationException("boom");
        var observed = new TaskCompletionSource<Exception>(TaskCreationOptions.RunContinuationsAsynchronously);
        var command = new ObservingAsyncCommand(
            () => Task.FromException(expected),
            exception =>
            {
                observed.TrySetResult(exception);
                return true;
            });

        command.Execute(null);

        var captured = await observed.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.Same(expected, captured);
    }

    [Fact]
    public async Task ExecuteRethrowsWhenOnExecuteExceptionReturnsFalse()
    {
        var expected = new InvalidOperationException("boom");
        var command = new ObservingAsyncCommand(
            () => Task.FromException(expected),
            _ => false);

        var rethrown = await CaptureAsyncVoidExceptionAsync(() => command.Execute(null));

        Assert.Same(expected, rethrown);
    }

    private sealed class ObservingAsyncCommand : AsyncCommand
    {
        private readonly Func<Exception, bool> _onException;

        public ObservingAsyncCommand(Func<Task> execute, Func<Exception, bool> onException)
        {
            ExecuteFunc = _ => execute();
            _onException = onException;
        }

        protected override bool OnExecuteException(Exception exception)
        {
            return _onException(exception);
        }
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
