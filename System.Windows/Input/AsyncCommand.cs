namespace Codify.System.Windows.Input;

public abstract class AsyncCommand : Command
{
    protected Func<object?, Task>? ExecuteFunc;

    public override async void Execute(object? parameter)
    {
        if (CanExecute(parameter) && ExecuteFunc is { } executeFunc) await executeFunc(parameter);
    }
}
