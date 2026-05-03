using System.Diagnostics.CodeAnalysis;

namespace Codify.System.Windows.Input;

/// <summary>
///     Provides a base implementation for synchronous commands.
/// </summary>
public abstract class SyncCommand : Command
{
    /// <summary>
    ///     Stores the delegate invoked by <see cref="Command.Execute(object?)" />.
    /// </summary>
    /// <remarks>
    ///     This field is intended to be assigned once during construction of the derived command and treated as
    ///     effectively read-only afterwards. Mutating the field after the command has been bound is not safe for
    ///     concurrent calls to <see cref="Execute(object?)" /> and is not protected by any synchronisation.
    /// </remarks>
    [SuppressMessage(
        "Design",
        "CA1051:Do not declare visible instance fields",
        Justification = "The protected field is part of the existing extensibility surface for derived command types.")]
    protected Action<object?>? ExecuteAction;

    /// <summary>
    ///     Executes the command when it can execute.
    /// </summary>
    /// <param name="parameter">The command parameter.</param>
    public override void Execute(object? parameter)
    {
        if (CanExecute(parameter))
        {
            ExecuteAction?.Invoke(parameter);
        }
    }
}
