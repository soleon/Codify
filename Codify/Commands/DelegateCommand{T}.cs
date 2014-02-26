using System;
using System.Windows.Input;
using Codify.Extensions;

namespace Codify.Windows.Commands
{
    /// <summary>
    ///     The DelegateCommand provides an implementation of <see cref="T:System.Windows.Input.ICommand" /> that can delegate
    ///     <see cref="ICommand.CanExecute">
    ///         CanExecute
    ///     </see>
    ///     and <see cref="ICommand.Execute">Execute</see> call to listeners with a generic parameter type.
    /// </summary>
    /// <typeparam name="T"> The parameter type </typeparam>
    public class DelegateCommand<T> : ICommand
    {
        #region Private Members

        private readonly Action<T> _executedAction;
        private readonly Func<T, bool> _canExecuteFunction;

        #endregion


        #region Constructors

        /// <summary>
        ///     Creates a new instance of DelegateCommand
        /// </summary>
        public DelegateCommand() {}

        /// <summary>
        ///     Creates a new instance of DelegatingCommand
        /// </summary>
        /// <param name="executedAction"> The action that will execute </param>
        public DelegateCommand(Action<T> executedAction)
        {
            _executedAction = executedAction;
        }

        /// <summary>
        ///     Creates a new instance of DelegatingCommand
        /// </summary>
        /// <param name="executedAction"> The action that will execute </param>
        /// <param name="canExecuteFunction"> The function that determines if command can execute </param>
        public DelegateCommand(Action<T> executedAction, Func<T, bool> canExecuteFunction)
        {
            _executedAction = executedAction;
            _canExecuteFunction = canExecuteFunction;
        }

        #endregion


        #region Events

        /// <summary>
        ///     Raised when the status of the CanExecute method changes
        /// </summary>
        public event EventHandler CanExecuteChanged;

        #endregion


        #region Private Methods

        private static T ConvertType(object parameter)
        {
            if (parameter is T)
            {
                return (T) parameter;
            }

            var defaultValue = default(T);

            if (parameter == null || !(defaultValue is IConvertible)) return defaultValue;
            var p = Convert.ChangeType(parameter, typeof (T), null);

            return p != null ? (T) p : defaultValue;
        }

        #endregion


        #region Public Methods

        /// <summary>
        ///     Determines if the command can execute in its current state.
        /// </summary>
        /// <param name="parameter"> </param>
        /// <returns> </returns>
        public bool CanExecute(T parameter)
        {
            return _canExecuteFunction == null || _canExecuteFunction(parameter);
        }

        /// <summary>
        ///     Determines if the command can execute in its current state.
        /// </summary>
        /// <param name="parameter"> </param>
        /// <returns> </returns>
        public bool CanExecute(object parameter)
        {
            return CanExecute(ConvertType(parameter));
        }

        /// <summary>
        ///     Executes the command with the given parameter
        /// </summary>
        /// <param name="parameter"> </param>
        public void Execute(T parameter)
        {
            if (CanExecute(parameter)) _executedAction.ExecuteIfNotNull(parameter);
        }

        /// <summary>
        ///     Executes the command with the given parameter
        /// </summary>
        /// <param name="parameter"> </param>
        public void Execute(object parameter)
        {
            Execute(ConvertType(parameter));
        }

        /// <summary>
        ///     Raises a notification that the CanExecute function has changed
        /// </summary>
        public void NotifyCanExecuteChanged()
        {
            CanExecuteChanged.ExecuteIfNotNull(this, EventArgs.Empty);
        }

        #endregion
    }
}