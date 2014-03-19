using System;
using System.Windows;
using Codify.Extensions;

namespace Codify.Windows.Entities
{
    /// <summary>
    ///     This is the base class of a view model that automatically creates and owns a view of type
    ///     <see cref="T:System.Windows.Window" />.
    /// </summary>
    public abstract class WindowViewModel<T> : ViewModel<T> where T : Window, new()
    {
        /// <summary>
        ///     Called when the of this view model is closed.
        /// </summary>
        private void OnViewClosed(object sender, EventArgs eventArgs)
        {
            ((T) sender).Closed -= OnViewClosed;
            Deactivate();
        }

        /// <summary>
        ///     Shows the window view.
        /// </summary>
        protected override void OnActivate()
        {
            base.OnActivate();
            View.UseIfNotNull(
                v =>
                {
                    v.Closed += OnViewClosed;
                    v.Owner = _owner;
                    Application.Current.UseIfNotNull(a =>
                    {
                        var action = (Action)(() =>
                        {
                            if (IsDialogWindow) v.ShowDialog();
                            else v.Show();
                        });
                        if(a.Dispatcher.CheckAccess()) action();
                        else a.Dispatcher.Invoke(action);
                    });
                });
        }

        /// <summary>
        ///     Hides the window view.
        /// </summary>
        protected override void OnDeactivate()
        {
            View.UseIfNotNull(v =>
            {
                var action = (Action) (() =>
                {
                    if (v.IsLoaded) v.Close();
                    v.Content = null;
                });
                if (v.Dispatcher.CheckAccess()) action();
                else v.Dispatcher.Invoke(action);
                View = null;
            });
            base.OnDeactivate();
        }

        /// <summary>
        ///     Shows the window view with the option to open it as a dialog window or a normal window.
        /// </summary>
        /// <param name="isDialogWindow">
        ///     if set to <c>true</c> shows the window view as a dialog window, otherwise, a normal window.
        /// </param>
        public void Activate(bool isDialogWindow = true)
        {
            IsDialogWindow = isDialogWindow;
            base.Activate();
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is dialog window. Change of this value is only effective
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is dialog window; otherwise, <c>false</c>.
        /// </value>
        public bool IsDialogWindow { get; private set; }

        /// <summary>
        ///     Gets or sets the <see cref="System.Windows.Window" /> that owns this System.Windows.Window.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.Windows.Window" /> object that represents the owner of this System.Windows.Window
        /// </returns>
        /// <exception cref="ArgumentException">A window tries to own itself-or-Two windows try to own each other.</exception>
        /// <exception cref="InvalidOperationException">
        ///     The System.Windows.Window.Owner property is set on a visible window shown
        ///     using System.Windows.Window.ShowDialog()-or-The System.Windows.Window.Owner property is set with a window that has
        ///     not been previously shown.
        /// </exception>
        public Window Owner
        {
            get { return _owner; }
            set
            {
                _owner = value;
                View.UseIfNotNull(v => v.Owner = value);
            }
        }

        private Window _owner;
    }
}