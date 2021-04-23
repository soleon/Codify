namespace Codify.System.ComponentModel
{
    public class ExpandableNotificationObject : NotificationObject
    {
        private bool _isExpanded;

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (SetValue(ref _isExpanded, value) && value)
                {
                    OnExpanded();
                }
            }
        }

        protected virtual void OnExpanded()
        {
        }
    }
}