using System;

namespace Codify.System.Windows.Input
{
    public abstract class SyncCommand : Command
    {
        protected Action<object> ExecuteAction;

        public override void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                ExecuteAction?.Invoke(parameter);
            }
        }
    }
}