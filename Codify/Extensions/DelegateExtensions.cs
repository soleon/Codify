using System.Windows.Input;
using Codify.Extensions;

namespace Codify.Windows.Extensions
{
    public static class DelegateExtensions
    {

        /// <summary>
        /// Executes the specified command if not null.
        /// </summary>
        /// <param name="target">The target command to be executed.</param>
        /// <param name="param">The command parameter.</param>
        public static void ExecuteIfNotNull(this ICommand target, object param = null)
        {
            target.UseIfNotNull(t => t.Execute(param));
        }
    }
}
