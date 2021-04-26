using System;
using System.Threading.Tasks;

namespace Codify.System.Windows.Input
{
    public sealed class AsyncActionCommand : AsyncCommand
    {
        public AsyncActionCommand(Func<Task> execute, Func<bool> canExecute = null)
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            ExecuteFunc = _ => CanExecute(null) ? execute() : Task.CompletedTask;
            CanExecuteFunc = _ => canExecute?.Invoke() ?? true;
        }
    }

    public sealed class AsyncActionCommand<T> : AsyncCommand
    {
        public AsyncActionCommand(Func<T, Task> execute, Func<T, bool> canExecute = null)
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            ExecuteFunc = param => param is T value
                ? CanExecute(value) ? execute(value) : Task.CompletedTask
                : throw new InvalidOperationException(
                    $"{param.GetType()} is not a valid parameter type for this command.");

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
}