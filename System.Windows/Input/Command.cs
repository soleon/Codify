using System.Windows.Input;

namespace Codify.System.Windows.Input;

public abstract class Command : ICommand
{
    protected Func<object, bool> CanExecuteFunc;

    public bool CanExecute(object parameter)
    {
        return CanExecuteFunc == null || CanExecuteFunc(parameter);
    }

    public abstract void Execute(object parameter);

    public event EventHandler CanExecuteChanged;

    public void NotifyCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}