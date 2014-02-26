using System;
using System.Linq;

namespace Codify.Extensions
{
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Gets all messages of all inner exceptions from the specified <see cref="AggregateException"/>.
        /// </summary>
        /// <param name="exception">The exception to get all the messages from.</param>
        /// <param name="separator">The separator between each message. The default value is <see cref="Environment.NewLine"/></param>
        public static string GetMessages(this AggregateException exception, string separator = null)
        {
            if (separator == null) separator = Environment.NewLine;
            return string.Join(separator, exception.Flatten().InnerExceptions.Select(exp => exp.Message));
        }
    }
}