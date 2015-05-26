using System;

namespace Codify.Extensions
{
    /// <summary>
    ///     Contains general purpose extensions that can be used by most objects.
    /// </summary>
    public static class GenericExtensions
    {
        /// <summary>
        ///     Applies the desired action on the target object if both of them are not null.
        /// </summary>
        /// <typeparam name="T">Type of the target object.</typeparam>
        /// <param name="target">The target to apply the action on.</param>
        /// <param name="action">The action to act on the target.</param>
        public static void UseIfNotNull<T>(this T target, Action<T> action) where T : class
        {
            if (target != null && action != null) action(target);
        }


        /// <summary>
        ///     Execute the specified action if the target is null.
        /// </summary>
        /// <param name="target">The target to check for nullity.</param>
        /// <param name="action">The action to execute if <see cref="target" /> is null.</param>
        public static void DoIfNull(this object target, Action action)
        {
            if (target == null && action != null) action();
        }

        /// <summary>
        ///     Processes the target and returns a value if the target is not null.
        /// </summary>
        /// <typeparam name="TTarget">The type of the target.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="target">The target to process if it is not null.</param>
        /// <param name="function">
        ///     The function that takes the target as an argument and produces a value of the type
        ///     <see cref="TResult" />.
        /// </param>
        /// <param name="defaultResult">
        ///     The optional default result to return either if the target or the processing function is
        ///     null, the default value of the type <see cref="TResult" /> is returned if this parameter is not specified.
        /// </param>
        /// <returns>
        ///     A value of type <see cref="TResult" /> produced by the specified <see cref="function" />, or the value of
        ///     <see cref="defaultResult" /> if it is specified, or the system default value for type <see cref="TResult" />.
        /// </returns>
        public static TResult ProcessIfNotNull<TTarget, TResult>(this TTarget target, Func<TTarget, TResult> function, TResult defaultResult = default(TResult)) where TTarget : class
        {
            return target == null || function == null ? defaultResult : function(target);
        }
    }
}