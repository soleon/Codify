using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace Codify.System.Windows.Input;

/// <summary>
///     Provides a base implementation of <see cref="global::System.Windows.Input.ICommand" />.
/// </summary>
public abstract class Command : ICommand
{
    /// <summary>
    ///     Stores the delegate invoked by <see cref="CanExecute(object?)" />.
    /// </summary>
    /// <remarks>
    ///     This field is intended to be assigned once during construction of the derived command and treated as
    ///     effectively read-only afterwards. Mutating the field after the command has been bound is not safe for
    ///     concurrent calls to <see cref="CanExecute(object?)" /> and is not protected by any synchronisation.
    /// </remarks>
    [SuppressMessage(
        "Design",
        "CA1051:Do not declare visible instance fields",
        Justification = "The protected field is part of the existing extensibility surface for derived command types.")]
    protected Func<object?, bool>? CanExecuteFunc;

    /// <summary>
    ///     Determines whether the command can execute with the specified parameter.
    /// </summary>
    /// <param name="parameter">The command parameter.</param>
    /// <returns><see langword="true" /> if the command can execute; otherwise, <see langword="false" />.</returns>
    public bool CanExecute(object? parameter)
    {
        return CanExecuteFunc == null || CanExecuteFunc(parameter);
    }

    /// <summary>
    ///     Occurs when changes affect whether the command should execute.
    /// </summary>
    public event EventHandler? CanExecuteChanged;

    /// <summary>
    ///     Executes the command with the specified parameter.
    /// </summary>
    /// <param name="parameter">The command parameter.</param>
    public abstract void Execute(object? parameter);

    /// <summary>
    ///     Raises the <see cref="CanExecuteChanged" /> event.
    /// </summary>
    public void NotifyCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}