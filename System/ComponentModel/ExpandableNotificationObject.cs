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
            if (SetValue(ref _isExpanded, value)) OnExpansionChanged(value);
        }
    }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (SetValue(ref _isSelected, value)) OnSelectionChanged(value);
        }
    }

    protected virtual void OnExpansionChanged(bool isExpended)
    {
    }

    protected virtual void OnSelectionChanged(bool isSelected)
    {
    }
}