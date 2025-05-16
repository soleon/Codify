namespace Codify.System.ComponentModel;

public class ExpandableNotificationObject : NotificationObject
{
    private bool _isExpanded;

    private bool _isSelected;

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
            OnExpansionChangedAsync(value);
        }
    }

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
            OnSelectionChangedAsync(value);
        }
    }

    protected virtual void OnExpansionChanged(bool isExpended)
    {
    }

    protected virtual Task OnExpansionChangedAsync(bool isExpended)
    {
        return Task.CompletedTask;
    }

    protected virtual void OnSelectionChanged(bool isSelected)
    {
    }

    protected virtual Task OnSelectionChangedAsync(bool isSelected)
    {
        return Task.CompletedTask;
    }
}