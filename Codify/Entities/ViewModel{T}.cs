using System.Windows;
using Codify.Entities;

namespace Codify.Windows.Entities
{
    /// <summary>
    ///     This is the base class of a view model that automatically creates and owns a view of type TView.
    /// </summary>
    /// <typeparam name="T"> The type of the view that corresponds to this view model. </typeparam>
    public abstract class ViewModel<T> : ViewModel where T : FrameworkElement, new()
    {
        #region Public Properties

        /// <summary>
        ///     Gets the view that corresponds to this view model.
        /// </summary>
        public virtual T View
        {
            get { return _view ?? (_view = new T()); }
            protected internal set { _view = value; }
        }

        private T _view;

        /// <summary>
        /// Gets the owner window of the <see cref="View"/> of this view model.
        /// </summary>
        public Window OwnerWindow
        {
            get { return Window.GetWindow(View); }
        }

        #endregion


        #region Event Handling

        /// <summary>
        ///     Called when <see cref="ViewModel.Dispose" /> method is executed on this view model.
        /// </summary>
        protected override void OnDispose()
        {
            View = null;
        }

        #endregion
    }
}