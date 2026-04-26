namespace Codify.System.Windows.Input;

/// <summary>
/// Provides a base implementation for synchronous commands.
/// </summary>
public abstract class SyncCommand : Command
{
    /// <summary>
    /// Stores the delegate invoked by <see cref="Execute(object?)" />.
    /// </summary>
    [global::System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Design",
        "CA1051:Do not declare visible instance fields",
        Justification = "The protected field is part of the existing extensibility surface for derived command types.")]
    protected Action<object?>? ExecuteAction;

    /// <summary>
    /// Executes the command when it can execute.
    /// </summary>
    /// <param name="parameter">The command parameter.</param>
    public override void Execute(object? parameter)
    {
        if (CanExecute(parameter)) ExecuteAction?.Invoke(parameter);
    }
}
