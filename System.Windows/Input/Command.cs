namespace Codify.System.Windows.Input;

/// <summary>
/// Provides a base implementation of <see cref="global::System.Windows.Input.ICommand" />.
/// </summary>
public abstract class Command : global::System.Windows.Input.ICommand
{
    /// <summary>
    /// Stores the delegate invoked by <see cref="CanExecute(object?)" />.
    /// </summary>
    /// <remarks>
    /// This field is intended to be assigned once during construction of the derived command and treated as
    /// effectively read-only afterwards. Mutating the field after the command has been bound is not safe for
    /// concurrent calls to <see cref="CanExecute(object?)" /> and is not protected by any synchronisation.
    /// </remarks>
    [global::System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Design",
        "CA1051:Do not declare visible instance fields",
        Justification = "The protected field is part of the existing extensibility surface for derived command types.")]
    protected global::System.Func<object?, bool>? CanExecuteFunc;

    /// <summary>
    /// Determines whether the command can execute with the specified parameter.
    /// </summary>
    /// <param name="parameter">The command parameter.</param>
    /// <returns><see langword="true" /> if the command can execute; otherwise, <see langword="false" />.</returns>
    public bool CanExecute(object? parameter)
    {
        return CanExecuteFunc == null || CanExecuteFunc(parameter);
    }

    /// <summary>
    /// Executes the command with the specified parameter.
    /// </summary>
    /// <param name="parameter">The command parameter.</param>
    public abstract void Execute(object? parameter);

    /// <summary>
    /// Occurs when changes affect whether the command should execute.
    /// </summary>
    public event global::System.EventHandler? CanExecuteChanged;

    /// <summary>
    /// Raises the <see cref="CanExecuteChanged" /> event.
    /// </summary>
    public void NotifyCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, global::System.EventArgs.Empty);
    }
}

internal static class CommandParameter<T>
{
    private static readonly bool AcceptsNull = default(T) is null;

    public static bool TryGetValue(object? parameter, out T value)
    {
        if (parameter is T typedValue)
        {
            value = typedValue;
            return true;
        }

        if (parameter == null && AcceptsNull)
        {
            value = default!;
            return true;
        }

        value = default!;
        return false;
    }

    public static bool IsNullInvalid(object? parameter)
    {
        return parameter == null && !AcceptsNull;
    }

    public static global::System.InvalidOperationException CreateInvalidTypeException(object parameter)
    {
        return new global::System.InvalidOperationException(
            $"{parameter.GetType()} is not a valid parameter type for this command. Expected {typeof(T)}.");
    }
}
