namespace Codify.System.ComponentModel;

/// <summary>
/// Provides observable expanded and selected state with synchronous and asynchronous change hooks.
/// </summary>
public class ExpandableNotificationObject : NotificationObject
{
    private bool _isExpanded;

    private bool _isSelected;

    /// <summary>
    /// Gets or sets a value indicating whether the object is expanded.
    /// </summary>
    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (!SetValue(ref _isExpanded, value))
            {
                return;
            }

            OnExpansionChanged(value);
            ObserveAsyncHook(OnExpansionChangedAsync(value));
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the object is selected.
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (!SetValue(ref _isSelected, value))
            {
                return;
            }

            OnSelectionChanged(value);
            ObserveAsyncHook(OnSelectionChangedAsync(value));
        }
    }

    /// <summary>
    /// Called synchronously after <see cref="IsExpanded" /> changes.
    /// </summary>
    /// <param name="isExpended">The new expanded state.</param>
    protected virtual void OnExpansionChanged(bool isExpended)
    {
    }

    /// <summary>
    /// Called after <see cref="IsExpanded"/> changes. The setter starts this hook without awaiting it;
    /// faulted returned tasks are observed by <see cref="OnAsyncHookException(Exception)"/>.
    /// </summary>
    /// <param name="isExpended">The new expanded state.</param>
    protected virtual Task OnExpansionChangedAsync(bool isExpended)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called synchronously after <see cref="IsSelected" /> changes.
    /// </summary>
    /// <param name="isSelected">The new selected state.</param>
    protected virtual void OnSelectionChanged(bool isSelected)
    {
    }

    /// <summary>
    /// Called after <see cref="IsSelected"/> changes. The setter starts this hook without awaiting it;
    /// faulted returned tasks are observed by <see cref="OnAsyncHookException(Exception)"/>.
    /// </summary>
    /// <param name="isSelected">The new selected state.</param>
    protected virtual Task OnSelectionChangedAsync(bool isSelected)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when a task returned by an asynchronous change hook faults.
    /// </summary>
    /// <param name="exception">The observed asynchronous hook exception.</param>
    protected virtual void OnAsyncHookException(Exception exception)
    {
    }

    private void ObserveAsyncHook(Task? task)
    {
        if (task == null || task.IsCompletedSuccessfully)
        {
            return;
        }

        _ = ObserveAsyncHookAsync(task);
    }

    private async Task ObserveAsyncHookAsync(Task task)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            try
            {
                OnAsyncHookException(exception);
            }
            catch
            {
                // Keep exception observation from surfacing on the continuation path.
            }
        }
    }
}
