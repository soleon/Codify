using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Codify.System.ComponentModel;

/// <summary>
/// Provides a base implementation of <see cref="INotifyPropertyChanged" /> for observable objects.
/// </summary>
public class NotificationObject : INotifyPropertyChanged
{
    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

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
    protected bool SetValue<T>(ref T source, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(source, value)) return false;

        source = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged" /> event for the specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
