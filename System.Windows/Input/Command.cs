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

internal static class CommandParameter<T>
{
    private static readonly bool AcceptsNull = default(T) is null;

    public static bool TryGetValue(object parameter, out T value)
    {
        if (parameter is T typedValue)
        {
            value = typedValue;
            return true;
        }

        if (parameter == null && AcceptsNull)
        {
            value = default;
            return true;
        }

        value = default;
        return false;
    }

    public static bool IsNullInvalid(object parameter)
    {
        return parameter == null && !AcceptsNull;
    }

    public static InvalidOperationException CreateInvalidTypeException(object parameter)
    {
        return new InvalidOperationException(
            $"{parameter.GetType()} is not a valid parameter type for this command. Expected {typeof(T)}.");
    }
}
