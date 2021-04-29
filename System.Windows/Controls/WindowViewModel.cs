using System;
using System.Windows;

namespace Codify.System.Windows.Controls
{
    public abstract class WindowViewModel<T> : ViewModel<T> where T : Window, new()
    {
        public bool? ShowDialog(Window owner = null)
        {
            var window = View;
            window.Owner = owner;
            return window.ShowDialog();
        }

        public void Show(Window owner = null)
        {
            var window = View;
            window.Owner = owner;
            window.Show();
        }

        public void Hide()
        {
            View.Hide();
        }

        public void Close()
        {
            View.Close();
        }

        public void Close(bool? dialogResult)
        {
            View.DialogResult = dialogResult;
        }

        protected override T CreateNewView()
        {
            var window = base.CreateNewView();

            void OnClosed(object _, EventArgs __)
            {
                window.Closed -= OnClosed;
                OnUnload();
                View = null;
            }

            window.Closed += OnClosed;

            return window;
        }
    }
}