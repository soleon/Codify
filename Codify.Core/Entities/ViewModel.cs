using System;

namespace Codify.Entities
{
    /// <summary>
    ///     This is the base class of a view model, who is responsible to create and manage it's corresponding view and
    /// </summary>
    public abstract class ViewModel : NotificationObject, IDisposable
    {
        #region Public Properties

        /// <summary>
        ///     Gets a value indicating whether this instance is disposed.
        /// </summary>
        /// <value> <c>true</c> if this instance is disposed; otherwise, <c>false</c> . </value>
        public bool IsDisposed { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether this instance is activated.
        /// </summary>
        /// <value> <c>true</c> if this instance is activated; otherwise, <c>false</c> . </value>
        public bool IsActivated { get; private set; }
        
        #endregion


        #region Public Methods

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed) return;
            Deactivate();
            OnDispose();
            IsDisposed = true;
        }

        /// <summary>
        ///     Activates this instance.
        /// </summary>
        public void Activate()
        {
            if (IsActivated || IsDisposed) return;
            OnActivate();
            IsActivated = true;
        }

        /// <summary>
        ///     Deactivates this instance.
        /// </summary>
        public void Deactivate()
        {
            if (!IsActivated) return;
            OnDeActivate();
            IsActivated = false;
        }

        #endregion


        #region Event Handling

        /// <summary>
        ///     Called when <see cref="Dispose" /> method is executed on this view model.
        /// </summary>
        protected virtual void OnDispose() { }

        /// <summary>
        ///     Called when <see cref="Activate" /> method is executed on this view model.
        /// </summary>
        protected virtual void OnActivate() { }

        /// <summary>
        ///     Called when <see cref="Deactivate" /> method is executed on this view model.
        /// </summary>
        protected virtual void OnDeActivate() { }

        #endregion
    }
}