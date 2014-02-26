using System;
using Codify.Entities;
using Codify.Extensions;
using Codify.Windows.Commands;

namespace Codify.Windows.Controls
{
    public class CustomMessageBoxButton : NotificationObject
    {
        internal event Action<object> Click;

        public CustomMessageBoxButton()
        {
            CommandClick = new DelegateCommand(() => Click.ExecuteIfNotNull(_result));
        }

        public object Content
        {
            get { return _content; }
            set { SetValue(ref _content, value, "Content"); }
        }

        private object _content;

        public object Result
        {
            get { return _result; }
            set { SetValue(ref _result, value, "Result"); }
        }

        private object _result;

        public bool IsDefault
        {
            get { return _isDefault; }
            set { SetValue(ref _isDefault, value, "IsDefault"); }
        }

        private bool _isDefault;

        public bool IsCancel
        {
            get { return _isCancel; }
            set { SetValue(ref _isCancel, value, "IsCancel"); }
        }

        private bool _isCancel;

        public DelegateCommand CommandClick { get; private set; }
    }
}