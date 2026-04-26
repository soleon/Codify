namespace Codify.System.Windows.Input;

/// <summary>
/// Provides a base implementation for asynchronous commands.
/// </summary>
public abstract class AsyncCommand : Command
{
    /// <summary>
    /// Stores the delegate invoked by <see cref="Command.Execute(object?)" />.
    /// </summary>
    [global::System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Design",
        "CA1051:Do not declare visible instance fields",
        Justification = "The protected field is part of the existing extensibility surface for derived command types.")]
    protected global::System.Func<object?, global::System.Threading.Tasks.Task>? ExecuteFunc;

    /// <summary>
    /// Executes the command asynchronously when it can execute.
    /// </summary>
    /// <param name="parameter">The command parameter.</param>
    public override async void Execute(object? parameter)
    {
        if (CanExecute(parameter) && ExecuteFunc is { } executeFunc) await executeFunc(parameter);
    }
}
