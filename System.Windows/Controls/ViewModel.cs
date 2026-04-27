using Codify.System.ComponentModel;

namespace Codify.System.Windows.Controls;

/// <summary>
/// Provides a base view model that lazily creates and owns a WPF view.
/// </summary>
/// <typeparam name="T">The type of WPF element managed by the view model.</typeparam>
public class ViewModel<T> : NotificationObject where T : global::System.Windows.FrameworkElement, new()
{
    private T? _view;

    /// <summary>
    /// Gets or sets the view associated with this view model.
    /// </summary>
    /// <remarks>
    /// The getter materializes a new view via <see cref="CreateNewView" /> the first time it is read after
    /// the view has been cleared, including after the setter is assigned <see langword="null" />.
    /// The setter is idempotent: assigning the current view does not raise
    /// <see cref="global::System.ComponentModel.INotifyPropertyChanged.PropertyChanged" />, does not detach
    /// or reattach lifecycle handlers, and does not reapply this view model as the view's data context.
    /// Assigning a different non-<see langword="null" /> view sets the new view's
    /// <see cref="global::System.Windows.FrameworkElement.DataContext" /> to this view model and then
    /// invokes <see cref="OnViewChanged" />.
    /// </remarks>
    [global::System.Diagnostics.CodeAnalysis.AllowNull]
    public virtual T View
    {
        get { return _view ??= CreateNewView(); }
        set
        {
            var old = _view;
            if (!SetValue(ref _view, value)) return;

            if (old != null)
            {
                old.Loaded -= OnLoaded;
                old.Unloaded -= OnUnloaded;
            }

            if (value != null)
            {
                value.DataContext = this;
                value.Loaded += OnLoaded;
                value.Unloaded += OnUnloaded;
            }

            OnViewChanged(old, value);
        }
    }

    /// <summary>
    /// Gets the currently assigned view without creating a new one.
    /// </summary>
    protected T? CurrentView => _view;

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

    /// <summary>
    /// Called after the associated view changes.
    /// </summary>
    /// <param name="oldView">The previous view.</param>
    /// <param name="newView">The new view.</param>
    protected virtual void OnViewChanged(T? oldView, T? newView)
    {
    }

    private void OnLoaded(object? sender, global::System.Windows.RoutedEventArgs args)
    {
        OnLoad();
    }

    private void OnUnloaded(object? sender, global::System.Windows.RoutedEventArgs args)
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
