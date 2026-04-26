namespace Codify.System.Windows.Input;

public sealed class AsyncActionCommand : AsyncCommand
{
    public AsyncActionCommand(Func<Task> execute, Func<bool> canExecute = null)
    {
        if (execute == null) throw new ArgumentNullException(nameof(execute));

        ExecuteFunc = _ => CanExecute(null) ? execute() : Task.CompletedTask;
        CanExecuteFunc = _ => canExecute?.Invoke() ?? true;
    }
}

public sealed class AsyncActionCommand<T> : AsyncCommand
{
    public AsyncActionCommand(Func<T, Task> execute, Func<T, bool> canExecute = null)
    {
        if (execute == null) throw new ArgumentNullException(nameof(execute));

        ExecuteFunc = param =>
        {
            if (CommandParameter<T>.TryGetValue(param, out var value))
            {
                return CanExecute(param) ? execute(value) : Task.CompletedTask;
            }

            return CommandParameter<T>.IsNullInvalid(param)
                ? Task.CompletedTask
                : throw CommandParameter<T>.CreateInvalidTypeException(param);
        };

        CanExecuteFunc = param =>
        {
            if (CommandParameter<T>.TryGetValue(param, out var value))
            {
                return canExecute == null || canExecute(value);
            }

            return CommandParameter<T>.IsNullInvalid(param)
                ? false
                : throw CommandParameter<T>.CreateInvalidTypeException(param);
        };
    }
}
