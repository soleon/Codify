namespace Codify.System.Windows.Input;

/// <summary>
/// Represents an asynchronous command that executes a parameterless task-returning delegate.
/// </summary>
public sealed class AsyncActionCommand : AsyncCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncActionCommand" /> class.
    /// </summary>
    /// <param name="execute">The asynchronous delegate to execute.</param>
    /// <param name="canExecute">An optional predicate that determines whether the command can execute.</param>
    public AsyncActionCommand(
        global::System.Func<global::System.Threading.Tasks.Task> execute,
        global::System.Func<bool>? canExecute = null)
    {
        global::System.ArgumentNullException.ThrowIfNull(execute);

        ExecuteFunc = _ => CanExecute(null) ? execute() : global::System.Threading.Tasks.Task.CompletedTask;
        CanExecuteFunc = _ => canExecute?.Invoke() ?? true;
    }
}

/// <summary>
/// Represents an asynchronous command that executes a task-returning delegate with a typed parameter.
/// </summary>
/// <typeparam name="T">The command parameter type.</typeparam>
public sealed class AsyncActionCommand<T> : AsyncCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncActionCommand{T}" /> class.
    /// </summary>
    /// <param name="execute">The asynchronous delegate to execute.</param>
    /// <param name="canExecute">An optional predicate that determines whether the command can execute.</param>
    public AsyncActionCommand(
        global::System.Func<T, global::System.Threading.Tasks.Task> execute,
        global::System.Func<T, bool>? canExecute = null)
    {
        global::System.ArgumentNullException.ThrowIfNull(execute);

        ExecuteFunc = param =>
        {
            if (CommandParameter<T>.TryGetValue(param, out var value))
            {
                return CanExecute(param) ? execute(value) : global::System.Threading.Tasks.Task.CompletedTask;
            }

            return CommandParameter<T>.IsNullInvalid(param)
                ? global::System.Threading.Tasks.Task.CompletedTask
                : throw CommandParameter<T>.CreateInvalidTypeException(param!);
        };

        CanExecuteFunc = param =>
        {
            if (CommandParameter<T>.TryGetValue(param, out var value))
            {
                return canExecute == null || canExecute(value);
            }

            return CommandParameter<T>.IsNullInvalid(param)
                ? false
                : throw CommandParameter<T>.CreateInvalidTypeException(param!);
        };
    }
}
