namespace Codify.System.Windows.Input;

public sealed class ActionCommand : SyncCommand
{
    public ActionCommand(Action execute, Func<bool> canExecute = null)
    {
        if (execute == null) throw new ArgumentNullException(nameof(execute));

        ExecuteAction = _ =>
        {
            if (CanExecute(null)) execute();
        };
        CanExecuteFunc = _ => canExecute?.Invoke() ?? true;
    }
}

public sealed class ActionCommand<T> : SyncCommand
{
    public ActionCommand(Action<T> execute, Func<T, bool> canExecute = null)
    {
        if (execute == null) throw new ArgumentNullException(nameof(execute));

        ExecuteAction = param =>
        {
            if (param is T value)
            {
                if (CanExecute(value)) execute(value);
            }
            else
            {
                throw new InvalidOperationException(
                    $"{param.GetType()} is not a valid parameter type for this command.");
            }
        };

        CanExecuteFunc = param =>
        {
            return param switch
            {
                null => true,
                T value => canExecute == null || canExecute(value),
                _ => throw new InvalidOperationException(
                    $"{param.GetType()} is not a valid parameter type for this command.")
            };
        };
    }
}