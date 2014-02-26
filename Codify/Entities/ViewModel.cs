using System.Windows;
using Codify.Entities;

namespace Codify.Windows.Entities
{
    /// <summary>
    ///     This is the base class of a view model that automatically creates and owns a view of type TView.
    /// </summary>
    /// <typeparam name="TView"> The type of the view that corresponds to this view model. </typeparam>
    public abstract class ViewModel<TView> : ViewModel where TView : FrameworkElement, new()
    {
        #region Public Properties

        /// <summary>
        ///     Gets the view that corresponds to this view model.
        /// </summary>
        public virtual TView View
        {
            get { return _view ?? (_view = new TView()); }
            protected internal set { _view = value; }
        }

        private TView _view;

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