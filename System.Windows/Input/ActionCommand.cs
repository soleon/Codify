namespace Codify.System.Windows.Input;

/// <summary>
/// Represents a synchronous command that executes a parameterless action.
/// </summary>
public sealed class ActionCommand : SyncCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ActionCommand" /> class.
    /// </summary>
    /// <param name="execute">The action to execute.</param>
    /// <param name="canExecute">An optional predicate that determines whether the command can execute.</param>
    public ActionCommand(global::System.Action execute, global::System.Func<bool>? canExecute = null)
    {
        global::System.ArgumentNullException.ThrowIfNull(execute);

        ExecuteAction = _ =>
        {
            if (CanExecute(null)) execute();
        };
        CanExecuteFunc = _ => canExecute?.Invoke() ?? true;
    }
}

/// <summary>
/// Represents a synchronous command that executes an action with a typed parameter.
/// </summary>
/// <typeparam name="T">The command parameter type.</typeparam>
public sealed class ActionCommand<T> : SyncCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ActionCommand{T}" /> class.
    /// </summary>
    /// <param name="execute">The action to execute.</param>
    /// <param name="canExecute">An optional predicate that determines whether the command can execute.</param>
    public ActionCommand(global::System.Action<T> execute, global::System.Func<T, bool>? canExecute = null)
    {
        global::System.ArgumentNullException.ThrowIfNull(execute);

        ExecuteAction = param =>
        {
            if (CommandParameter<T>.TryGetValue(param, out var value))
            {
                if (CanExecute(param)) execute(value);
            }
            else if (!CommandParameter<T>.IsNullInvalid(param))
            {
                throw CommandParameter<T>.CreateInvalidTypeException(param!);
            }
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
