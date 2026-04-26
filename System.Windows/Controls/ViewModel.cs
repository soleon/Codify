using System.Windows;
using Codify.System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Codify.System.Windows.Controls;

/// <summary>
/// Provides a base view model that lazily creates and owns a WPF view.
/// </summary>
/// <typeparam name="T">The type of WPF element managed by the view model.</typeparam>
public class ViewModel<T> : NotificationObject where T : FrameworkElement, new()
{
    private T? _view;

    /// <summary>
    /// Gets or sets the view associated with this view model.
    /// </summary>
    [AllowNull]
    public virtual T View
    {
        get { return _view ??= CreateNewView(); }
        set
        {
            var old = _view;
            if (!SetValue(ref _view, value)) return;

            old?.Loaded -= OnLoaded;
            old?.Unloaded -= OnUnloaded;
            value?.Loaded += OnLoaded;
            value?.Unloaded += OnUnloaded;
        }
    }

    /// <summary>
    /// Creates a new view instance and attaches this view model as its data context.
    /// </summary>
    /// <returns>The newly created view.</returns>
    protected virtual T CreateNewView()
    {
        var view = new T { DataContext = this };

        view.Loaded += OnLoaded;
        view.Unloaded += OnUnloaded;

        return view;
    }

    private void OnLoaded(object? sender, RoutedEventArgs args)
    {
        OnLoad();
    }

    private void OnUnloaded(object? sender, RoutedEventArgs args)
    {
        OnUnload();
    }

    /// <summary>
    /// Called when the associated view is loaded.
    /// </summary>
    protected virtual void OnLoad()
    {
    }

    /// <summary>
    /// Called when the associated view is unloaded.
    /// </summary>
    protected virtual void OnUnload()
    {
    }
}
