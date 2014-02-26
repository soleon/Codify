using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Codify.Windows.Threading.Tasks
{
    /// <summary>
    ///     This is needed when Using the Task Parallel Library functions as there is a bug in the .net 4.0 framework which
    ///     causes TaskScheduler.FromCurrentSynchronizationContext() to be null randomly.
    /// </summary>
    public static class UITaskScheduler
    {
        private static readonly Lazy<TaskScheduler> LazyTaskScheduler;

        static UITaskScheduler()
        {
            LazyTaskScheduler = new Lazy<TaskScheduler>(() =>
            {
                TaskScheduler scheduler = null;
                var dispatcher = Application.Current.Dispatcher;
                if (dispatcher.CheckAccess())
                    return TaskScheduler.FromCurrentSynchronizationContext();
                dispatcher.Invoke((Action)(() => scheduler = TaskScheduler.FromCurrentSynchronizationContext()));
                return scheduler;
            }, LazyThreadSafetyMode.PublicationOnly);
        }

        /// <summary>
        ///     Returns a <see cref="TaskScheduler" /> that is created on the UI/main thread.
        /// </summary>
        /// <remarks>
        ///     Note, you only need to use this if you are doing UI related operations, otherwise, this would be a slight
        ///     performance hit.
        /// </remarks>
        public static TaskScheduler FromCurrentSynchronizationContext()
        {
            return LazyTaskScheduler.Value;
        }
    }
}