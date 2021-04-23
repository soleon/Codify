using System;
using System.Threading.Tasks;

namespace Codify.System.Windows.Input
{
    public abstract class AsyncCommand : Command
    {
        protected Func<object, Task> ExecuteFunc;

        public override async void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                await ExecuteFunc(parameter);
            }
        }
    }
}