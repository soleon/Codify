using System.Windows;

namespace Codify.System.Windows.Controls;

/// <summary>
///     Provides a base view model that manages a WPF window.
/// </summary>
/// <typeparam name="T">The type of window managed by the view model.</typeparam>
public abstract class WindowViewModel<T> : ViewModel<T> where T : Window, new()
{
    /// <summary>
    ///     Closes the associated window.
    /// </summary>
    public void Close()
    {
        View.Close();
    }

    /// <summary>
    ///     Sets the dialog result for the associated window.
    /// </summary>
    /// <param name="dialogResult">The dialog result to assign.</param>
    public void Close(bool? dialogResult)
    {
        View.DialogResult = dialogResult;
    }

    /// <summary>
    ///     Creates a new window instance and resets the view after it closes.
    /// </summary>
    /// <returns>The newly created window.</returns>
    protected override T CreateNewView()
    {
        T window = base.CreateNewView();
        window.Closed += OnClosed;
        return window;
    }

    /// <summary>
    ///     Hides the associated window.
    /// </summary>
    public void Hide()
    {
        View.Hide();
    }

    private void OnClosed(object? sender, EventArgs args)
    {
        if (!ReferenceEquals(sender, CurrentView))
        {
            return;
        }

        OnUnload();
        View = null;
    }

    /// <summary>
    ///     Updates window lifecycle subscriptions when the associated view changes.
    /// </summary>
    /// <param name="oldView">The previous window.</param>
    /// <param name="newView">The new window.</param>
    protected override void OnViewChanged(T? oldView, T? newView)
    {
        if (oldView != null)
        {
            oldView.Closed -= OnClosed;
        }

        if (newView != null)
        {
            newView.Closed += OnClosed;
        }
    }

    /// <summary>
    ///     Shows the window non-modally.
    /// </summary>
    /// <param name="owner">The owner window to assign before showing the window.</param>
    public void Show(Window? owner = null)
    {
        T window = View;
        window.Owner = owner;
        window.Show();
    }

    /// <summary>
    ///     Shows the window modally.
    /// </summary>
    /// <param name="owner">The owner window to assign before showing the dialog.</param>
    /// <returns>The dialog result returned by the window.</returns>
    public bool? ShowDialog(Window? owner = null)
    {
        T window = View;
        window.Owner = owner;
        return window.ShowDialog();
    }
}
