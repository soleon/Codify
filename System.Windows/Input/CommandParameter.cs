namespace Codify.System.Windows.Input;

internal static class CommandParameter<T>
{
    private static readonly bool AcceptsNull = default(T) is null;

    public static InvalidOperationException CreateInvalidTypeException(object parameter)
    {
        return new InvalidOperationException(
            $"{parameter.GetType()} is not a valid parameter type for this command. Expected {typeof(T)}.");
    }

    public static bool IsNullInvalid(object? parameter)
    {
        return parameter == null && !AcceptsNull;
    }

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
}
