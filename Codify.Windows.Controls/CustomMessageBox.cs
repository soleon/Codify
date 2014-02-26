using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Codify.Extensions;

namespace Codify.Windows.Controls
{
    public class CustomMessageBox : Window
    {
        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            CustomMessageBoxButton[] buttons;
            MessageBoxResult defaultResult;
            switch (button)
            {
                case MessageBoxButton.OK:
                    buttons = new[]
                    {
                        new CustomMessageBoxButton
                        {
                            Content = "OK",
                            IsDefault = true,
                            IsCancel = true,
                            Result = defaultResult = MessageBoxResult.OK
                        }
                    };
                    break;
                case MessageBoxButton.OKCancel:
                    buttons = new[]
                    {
                        new CustomMessageBoxButton
                        {
                            Content = "OK",
                            IsDefault = true,
                            Result = MessageBoxResult.OK
                        },
                        new CustomMessageBoxButton
                        {
                            Content = "Cancel",
                            IsCancel = true,
                            Result = defaultResult = MessageBoxResult.Cancel
                        }
                    };
                    break;
                case MessageBoxButton.YesNoCancel:
                    buttons = new[]
                    {
                        new CustomMessageBoxButton
                        {
                            Content = "Yes",
                            IsDefault = true,
                            Result = MessageBoxResult.Yes
                        },
                        new CustomMessageBoxButton
                        {
                            Content = "No",
                            Result = MessageBoxResult.No
                        },
                        new CustomMessageBoxButton
                        {
                            Content = "Cancel",
                            IsCancel = true,
                            Result = defaultResult = MessageBoxResult.Cancel
                        }
                    };
                    break;
                case MessageBoxButton.YesNo:
                    buttons = new[]
                    {
                        new CustomMessageBoxButton
                        {
                            Content = "Yes",
                            IsDefault = true,
                            Result = MessageBoxResult.Yes
                        },
                        new CustomMessageBoxButton
                        {
                            Content = "No",
                            IsCancel = true,
                            Result = defaultResult = MessageBoxResult.No
                        }
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException("button");
            }
            return (MessageBoxResult) Show(messageBoxText, caption, buttons, icon, defaultResult);
        }

        public static object Show(string messageBoxText, string caption, params CustomMessageBoxButton[] buttons)
        {
            return Show(messageBoxText, caption, buttons, MessageBoxImage.None);
        }

        public static object Show(string messageBoxText, string caption, IEnumerable<CustomMessageBoxButton> buttons, MessageBoxImage icon)
        {
            return Show(messageBoxText, caption, buttons, icon, null);
        }

        public static object Show(string messageBoxText, string caption, IEnumerable<CustomMessageBoxButton> buttons, MessageBoxImage icon, object defaultResult)
        {
            var owner = Application.Current.Windows.Cast<Window>().FirstOrDefault(w => w.IsActive);
            return Show(owner, messageBoxText, caption, buttons, icon, defaultResult);
        }

        public static object Show(Window owner, string messageBoxText, string caption, params CustomMessageBoxButton[] buttons)
        {
            return Show(owner, messageBoxText, caption, buttons, MessageBoxImage.None);
        }

        public static object Show(Window owner, string messageBoxText, string caption, IEnumerable<CustomMessageBoxButton> buttons, MessageBoxImage icon)
        {
            return Show(owner, messageBoxText, caption, buttons, icon, null);
        }

        public static object Show(Window owner, string messageBoxText, string caption, IEnumerable<CustomMessageBoxButton> buttons, MessageBoxImage icon, object defaultResult)
        {
            Icon systemIcon = null;
            switch (icon)
            {
                case MessageBoxImage.Information:
                    systemIcon = SystemIcons.Information;
                    break;
                case MessageBoxImage.Error:
                    systemIcon = SystemIcons.Error;
                    break;
                case MessageBoxImage.Exclamation:
                    systemIcon = SystemIcons.Exclamation;
                    break;
                case MessageBoxImage.Question:
                    systemIcon = SystemIcons.Question;
                    break;
            }
            var messageBox = new CustomMessageBox
            {
                Content = messageBoxText,
                Title = caption,
                Buttons = buttons,
                IconImageSource = systemIcon.ProcessIfNotNull(i => Imaging.CreateBitmapSourceFromHIcon(i.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())),
                DefaultResult = defaultResult,
                Owner = owner,
                WindowStartupLocation = owner == null ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner
            };
            messageBox.ShowDialog();
            return messageBox.Result;
        }

        public CustomMessageBox()
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            DefaultStyleKey = typeof (CustomMessageBox);
        }

        public IEnumerable<CustomMessageBoxButton> Buttons
        {
            get { return (IEnumerable<CustomMessageBoxButton>) GetValue(ButtonsProperty); }
            set { SetValue(ButtonsProperty, value); }
        }

        public static readonly DependencyProperty ButtonsProperty = DependencyProperty.Register(
            "Buttons",
            typeof (IEnumerable<CustomMessageBoxButton>),
            typeof (CustomMessageBox),
            new PropertyMetadata(default(IEnumerable<CustomMessageBoxButton>), (s, e) => ((CustomMessageBox) s).OnButtonsChanged((IEnumerable<CustomMessageBoxButton>) e.OldValue, (IEnumerable<CustomMessageBoxButton>) e.NewValue)));

        private void OnButtonsChanged(IEnumerable<CustomMessageBoxButton> oldValue, IEnumerable<CustomMessageBoxButton> newValue)
        {
            if (oldValue != null)
                oldValue.Where(button => button != null).ForEach(button => button.Click -= OnButtonClick);
            if (newValue != null)
                newValue.Where(button => button != null).ForEach(button => button.Click += OnButtonClick);
        }

        public ImageSource IconImageSource
        {
            get { return (ImageSource) GetValue(IconImageSourceProperty); }
            set { SetValue(IconImageSourceProperty, value); }
        }

        public static readonly DependencyProperty IconImageSourceProperty = DependencyProperty.Register("IconImageSource", typeof (ImageSource), typeof (CustomMessageBox));


        #region DefaultResult

        public object DefaultResult
        {
            get { return GetValue(DefaultResultProperty); }
            set { SetValue(DefaultResultProperty, value); }
        }

        public static readonly DependencyProperty DefaultResultProperty = DependencyProperty.Register(
            "DefaultResult",
            typeof (object),
            typeof (CustomMessageBox),
            new PropertyMetadata(default(object), (s, e) => ((CustomMessageBox) s).OnDefaultResultChanged(e.OldValue, e.NewValue)));

        private void OnDefaultResultChanged(object oldValue, object newValue)
        {
            if (Result == oldValue)
                Result = newValue;
        }

        #endregion


        public object Result { get; private set; }

        private void OnButtonClick(object result)
        {
            Result = result;
            Close();
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key != Key.Escape) return;
            Result = DefaultResult;
            Close();
        }
    }
}