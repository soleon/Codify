using System;

namespace Codify.Extensions
{
    /// <summary>
    ///     Contains extensions for delegate types.
    /// </summary>
    public static class DelegateExtensions
    {
        /// <summary>
        ///     Executes the specified action if not null.
        /// </summary>
        /// <param name="target">The target action to be executed.</param>
        public static void ExecuteIfNotNull(this Action target)
        {
            target.UseIfNotNull(a => a());
        }

        /// <summary>
        ///     Executes the event handler if not null.
        /// </summary>
        /// <param name="handler">The handler to be executed.</param>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        public static void ExecuteIfNotNull(this EventHandler handler, object sender, EventArgs e)
        {
            handler.UseIfNotNull(h => h(sender, e));
        }

        /// <summary>
        ///     Executes the event handler if not null.
        /// </summary>
        /// <param name="handler">The handler to be executed.</param>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        public static void ExecuteIfNotNull<TEventArgs>(this EventHandler<TEventArgs> handler, object sender, TEventArgs e) where TEventArgs : EventArgs
        {
            handler.UseIfNotNull(h => h(sender, e));
        }

        /// <summary>
        ///     Executes the specified action if not null.
        /// </summary>
        /// <typeparam name="T">Typr of the parameter of the action.</typeparam>
        /// <param name="action">The action to be executed.</param>
        /// <param name="parameter">The parameter of the action.</param>
        public static void ExecuteIfNotNull<T>(this Action<T> action, T parameter)
        {
            action.UseIfNotNull(a => a(parameter));
        }

        /// <summary>
        ///     Executes if not null.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the action.</typeparam>
        /// <typeparam name="T2">The type of the second parameter fo the action.</typeparam>
        /// <param name="action">The action to be executed.</param>
        /// <param name="param1">The first parameter of the action.</param>
        /// <param name="param2">The second parameter of the action.</param>
        public static void ExecuteIfNotNull<T1, T2>(this Action<T1, T2> action, T1 param1, T2 param2)
        {
            action.UseIfNotNull(a => a(param1, param2));
        }

        /// <summary>
        ///     Executes the function if not null.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="function">The function to be executed.</param>
        /// <param name="defaultResult">Optional. The default result to be returned if the function is null.</param>
        /// <returns>
        ///     A value of type <see cref="TResult" /> produced by the specified <see cref="function" />, or the value of
        ///     <see cref="defaultResult" /> if it is specified, or the system default value for type <see cref="TResult" />.
        /// </returns>
        public static TResult ExecuteIfNotNull<TResult>(this Func<TResult> function, TResult defaultResult = default(TResult))
        {
            return function.ProcessIfNotNull(f => f(), defaultResult);
        }

        /// <summary>
        ///     Executes the function if not null.
        /// </summary>
        /// <typeparam name="T">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="function">The function to be executed.</param>
        /// <param name="param">The first parameter of the function.</param>
        /// <param name="defaultResult">Optional. The default result to be returned if the function is null.</param>
        /// <returns>
        ///     A value of type <see cref="TResult" /> produced by the specified <see cref="function" />, or the value of
        ///     <see cref="defaultResult" /> if it is specified, or the system default value for type <see cref="TResult" />.
        /// </returns>
        public static TResult ExecuteIfNotNull<T, TResult>(this Func<T, TResult> function, T param, TResult defaultResult = default(TResult))
        {
            return function.ProcessIfNotNull(f => f(param), defaultResult);
        }

        /// <summary>
        ///     Executes the function if not null.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="function">The function to be executed.</param>
        /// <param name="param1">The first parameter of the function.</param>
        /// <param name="param2">The second parameter of the function.</param>
        /// <param name="defaultResult">Optional. The default result to be returned if the function is null.</param>
        /// <returns>
        ///     A value of type <see cref="TResult" /> produced by the specified <see cref="function" />, or the value of
        ///     <see cref="defaultResult" /> if it is specified, or the system default value for type <see cref="TResult" />.
        /// </returns>
        public static TResult ExecuteIfNotNull<T1, T2, TResult>(this Func<T1, T2, TResult> function, T1 param1, T2 param2, TResult defaultResult = default(TResult))
        {
            return function.ProcessIfNotNull(f => f(param1, param2), defaultResult);
        }

        /// <summary>
        ///     Executes the function if not null.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="T3">The type of the thrid parameter of the function.</typeparam>
        /// <typeparam name="T4">The type of the forth parameter of the function.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="function">The function to be executed.</param>
        /// <param name="param1">The first parameter of the function.</param>
        /// <param name="param2">The second parameter of the function.</param>
        /// <param name="param3">The thrid parameter of the function.</param>
        /// <param name="param4">The forth parameter of the function.</param>
        /// <param name="defaultResult">Optional. The default result to be returned if the function is null.</param>
        /// <returns>
        ///     A value of type <see cref="TResult" /> produced by the specified <see cref="function" />, or the value of
        ///     <see cref="defaultResult" /> if it is specified, or the system default value for type <see cref="TResult" />.
        /// </returns>
        public static TResult ExecuteIfNotNull<T1, T2, T3, T4, TResult>(this Func<T1, T2, T3, T4, TResult> function, T1 param1, T2 param2, T3 param3, T4 param4, TResult defaultResult = default(TResult))
        {
            return function.ProcessIfNotNull(f => f(param1, param2, param3, param4), defaultResult);
        }
    }
}