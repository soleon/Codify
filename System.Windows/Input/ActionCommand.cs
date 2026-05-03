namespace Codify.System.Windows.Input;

/// <summary>
///     Represents a synchronous command that executes a parameterless action.
/// </summary>
public sealed class ActionCommand : SyncCommand
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ActionCommand" /> class.
    /// </summary>
    /// <param name="execute">The action to execute.</param>
    /// <param name="canExecute">An optional predicate that determines whether the command can execute.</param>
    public ActionCommand(Action execute, Func<bool>? canExecute = null)
    {
        ArgumentNullException.ThrowIfNull(execute);

        ExecuteAction = _ => execute();
        CanExecuteFunc = _ => canExecute?.Invoke() ?? true;
    }
}

/// <summary>
///     Represents a synchronous command that executes an action with a typed parameter.
/// </summary>
/// <typeparam name="T">The command parameter type.</typeparam>
public sealed class ActionCommand<T> : SyncCommand
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ActionCommand{T}" /> class.
    /// </summary>
    /// <param name="execute">The action to execute.</param>
    /// <param name="canExecute">An optional predicate that determines whether the command can execute.</param>
    public ActionCommand(Action<T> execute, Func<T, bool>? canExecute = null)
    {
        ArgumentNullException.ThrowIfNull(execute);

        ExecuteAction = param =>
        {
            if (CommandParameter<T>.TryGetValue(param, out T value))
            {
                execute(value);
            }
            else if (!CommandParameter<T>.IsNullInvalid(param))
            {
                throw CommandParameter<T>.CreateInvalidTypeException(param!);
            }
        };

        CanExecuteFunc = param =>
        {
            if (CommandParameter<T>.TryGetValue(param, out T value))
            {
                return canExecute == null || canExecute(value);
            }

            return CommandParameter<T>.IsNullInvalid(param)
                ? false
                : throw CommandParameter<T>.CreateInvalidTypeException(param!);
        };
    }
}
