namespace Codify.System.ComponentModel;

/// <summary>
/// Provides a base implementation of <see cref="global::System.ComponentModel.INotifyPropertyChanged" /> for observable objects.
/// </summary>
public class NotificationObject : global::System.ComponentModel.INotifyPropertyChanged
{
    private const int MaxCachedPropertyChangedEventArgs = 1024;

    private static readonly global::System.ComponentModel.PropertyChangedEventArgs NullPropertyChangedEventArgs =
        new(null);

    private static readonly global::System.Collections.Concurrent.ConcurrentDictionary<
        string,
        global::System.ComponentModel.PropertyChangedEventArgs> PropertyChangedEventArgsCache =
        new(global::System.StringComparer.Ordinal);

    private static readonly global::System.Threading.Lock PropertyChangedEventArgsCacheLock = new();

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event global::System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Sets a backing field and raises change notifications when the value changes.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="source">The backing field to update.</param>
    /// <param name="value">The new value.</param>
    /// <param name="propertyName">The name of the primary property that changed.</param>
    /// <param name="propertyNames">Additional dependent property names to notify.</param>
    /// <returns><see langword="true" /> if the value changed; otherwise, <see langword="false" />.</returns>
    protected bool SetValue<T>(ref T source, T value, string propertyName, params string[]? propertyNames)
    {
        if (!SetValue(ref source, value, propertyName)) return false;

        if (propertyNames == null) return true;

        foreach (var name in propertyNames) OnPropertyChanged(name);

        return true;
    }

    /// <summary>
    /// Sets a backing field and raises a property change notification when the value changes.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="source">The backing field to update.</param>
    /// <param name="value">The new value.</param>
    /// <param name="propertyName">The name of the property that changed.</param>
    /// <returns><see langword="true" /> if the value changed; otherwise, <see langword="false" />.</returns>
    protected bool SetValue<T>(
        ref T source,
        T value,
        [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        if (global::System.Collections.Generic.EqualityComparer<T>.Default.Equals(source, value)) return false;

        source = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged" /> event for the specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    protected virtual void OnPropertyChanged(
        [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, GetPropertyChangedEventArgs(propertyName));
    }

    private static global::System.ComponentModel.PropertyChangedEventArgs GetPropertyChangedEventArgs(
        string? propertyName)
    {
        if (propertyName is null)
        {
            return NullPropertyChangedEventArgs;
        }

        if (PropertyChangedEventArgsCache.TryGetValue(propertyName, out var args))
        {
            return args;
        }

        lock (PropertyChangedEventArgsCacheLock)
        {
            if (PropertyChangedEventArgsCache.TryGetValue(propertyName, out args))
            {
                return args;
            }

            if (PropertyChangedEventArgsCache.Count >= MaxCachedPropertyChangedEventArgs)
            {
                return new global::System.ComponentModel.PropertyChangedEventArgs(propertyName);
            }

            args = new global::System.ComponentModel.PropertyChangedEventArgs(propertyName);
            PropertyChangedEventArgsCache[propertyName] = args;
            return args;
        }
    }
}
