namespace Codify.System.Windows.Input;

public abstract class SyncCommand : Command
{
    [global::System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Design",
        "CA1051:Do not declare visible instance fields",
        Justification = "The protected field is part of the existing extensibility surface for derived command types.")]
    protected Action<object?>? ExecuteAction;

    public override void Execute(object? parameter)
    {
        if (CanExecute(parameter)) ExecuteAction?.Invoke(parameter);
    }
}
