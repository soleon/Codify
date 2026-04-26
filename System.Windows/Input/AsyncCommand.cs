namespace Codify.System.Windows.Input;

public abstract class AsyncCommand : Command
{
    [global::System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Design",
        "CA1051:Do not declare visible instance fields",
        Justification = "The protected field is part of the existing extensibility surface for derived command types.")]
    protected Func<object?, Task>? ExecuteFunc;

    public override async void Execute(object? parameter)
    {
        if (CanExecute(parameter) && ExecuteFunc is { } executeFunc) await executeFunc(parameter);
    }
}
