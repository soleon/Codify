namespace Codify.System.Windows.Input;

public sealed class ActionCommand : SyncCommand
{
    public ActionCommand(Action execute, Func<bool>? canExecute = null)
    {
        ArgumentNullException.ThrowIfNull(execute);

        ExecuteAction = _ =>
        {
            if (CanExecute(null)) execute();
        };
        CanExecuteFunc = _ => canExecute?.Invoke() ?? true;
    }
}

public sealed class ActionCommand<T> : SyncCommand
{
    public ActionCommand(Action<T> execute, Func<T, bool>? canExecute = null)
    {
        ArgumentNullException.ThrowIfNull(execute);

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
