using System;
using System.Windows;
using Codify.System.ComponentModel;

namespace Codify.System.Windows.Controls
{
    public abstract class WindowViewModel<T> : NotificationObject where T : Window, new()
    {
        private T _window;

        public bool? ShowDialog(Window owner = null)
        {
            return GetWindow(owner).ShowDialog();
        }

        public void Show(Window owner = null)
        {
            GetWindow(owner).Show();
        }

        protected internal T GetWindow(Window owner = null)
        {
            if (_window != null)
            {
                if (owner != null)
                {
                    _window.Owner = owner;
                }

                return _window;
            }

            _window = new T {DataContext = this, Owner = owner};

            void OnWindowOnClosed(object _, EventArgs __)
            {
                _window.Closed -= OnWindowOnClosed;
                _window = null;
                OnClosed();
            }

            _window.Closed += OnWindowOnClosed;

            return _window;
        }

        protected virtual void OnClosed()
        {
        }
    }
}