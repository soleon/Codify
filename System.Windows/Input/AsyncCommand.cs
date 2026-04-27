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
        if (!CanExecute(parameter) || ExecuteFunc is not { } executeFunc)
        {
            return;
        }

        try
        {
            await executeFunc(parameter);
        }
        catch (global::System.Exception exception)
        {
            bool handled;
            try
            {
                handled = OnExecuteException(exception);
            }
            catch
            {
                handled = false;
            }

            if (!handled)
            {
                throw;
            }
        }
    }

    /// <summary>
    /// Invoked when the awaited delegate throws. Override to observe or handle command exceptions.
    /// </summary>
    /// <param name="exception">The exception thrown by the awaited delegate.</param>
    /// <returns>
    /// <see langword="true" /> if the exception was handled and should not propagate further;
    /// <see langword="false" /> to rethrow on the captured synchronisation context (default behaviour).
    /// </returns>
    protected virtual bool OnExecuteException(global::System.Exception exception)
    {
        return false;
    }
}
