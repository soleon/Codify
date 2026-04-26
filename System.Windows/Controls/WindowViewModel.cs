using System.Windows;

namespace Codify.System.Windows.Controls;

/// <summary>
/// Provides a base view model that manages a WPF window.
/// </summary>
/// <typeparam name="T">The type of window managed by the view model.</typeparam>
public abstract class WindowViewModel<T> : ViewModel<T> where T : Window, new()
{
    /// <summary>
    /// Shows the window modally.
    /// </summary>
    /// <param name="owner">The owner window to assign before showing the dialog.</param>
    /// <returns>The dialog result returned by the window.</returns>
    public bool? ShowDialog(Window? owner = null)
    {
        var window = View;
        window.Owner = owner;
        return window.ShowDialog();
    }

    /// <summary>
    /// Shows the window non-modally.
    /// </summary>
    /// <param name="owner">The owner window to assign before showing the window.</param>
    public void Show(Window? owner = null)
    {
        var window = View;
        window.Owner = owner;
        window.Show();
    }

    /// <summary>
    /// Hides the associated window.
    /// </summary>
    public void Hide()
    {
        View.Hide();
    }

    /// <summary>
    /// Closes the associated window.
    /// </summary>
    public void Close()
    {
        View.Close();
    }

    /// <summary>
    /// Sets the dialog result for the associated window.
    /// </summary>
    /// <param name="dialogResult">The dialog result to assign.</param>
    public void Close(bool? dialogResult)
    {
        View.DialogResult = dialogResult;
    }

    /// <summary>
    /// Creates a new window instance and resets the view after it closes.
    /// </summary>
    /// <returns>The newly created window.</returns>
    protected override T CreateNewView()
    {
        var window = base.CreateNewView();

        void OnClosed(object? _, EventArgs __)
        {
            window.Closed -= OnClosed;
            OnUnload();
            View = null;
        }

        window.Closed += OnClosed;

        return window;
    }
}
