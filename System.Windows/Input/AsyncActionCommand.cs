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
}