using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Codify.Windows.Extensions
{
    /// <summary>
    /// A collection of extension methods to aggregate calls to an asynchronous function.
    /// 
    /// If the function is called while the process is still going, the same <see cref="Task"/>
    /// will be returned to the caller.
    /// 
    /// If the function is called with a minimum interval, and the time interval between this call and the completion of the last call
    /// is shorter then the minimum interval, a new <see cref="Task"/> is generated and returned immediately, this task then waits until
    /// the minimum interval is elapsed before executing the actual function.
    /// </summary>
    public static class ConcurrentExecutionExtensions
    {
        private static readonly ConcurrentDictionary<MulticastDelegate, DelegateContext> DelegateContexts = new ConcurrentDictionary<MulticastDelegate, DelegateContext>();

        public static Task Aggregate(this Func<Task> func, TimeSpan? minimumInterval = null, TaskScheduler scheduler = null, params object[] parameters)
        {
            return Aggregate<object, Task>(func, minimumInterval, scheduler, parameters);
        }

        public static Task<TResult> Aggregate<TResult>(this Func<Task<TResult>> func, TimeSpan? minimumInterval = null, TaskScheduler scheduler = null)
        {
            return Aggregate<TResult, Task<TResult>>(func, minimumInterval, scheduler, null);
        }

        public static Task<TResult> Aggregate<T, TResult>(this Func<T, Task<TResult>> func, T arg, TimeSpan? minimumInterval = null, TaskScheduler scheduler = null)
        {
            return Aggregate<TResult, Task<TResult>>(func, minimumInterval, scheduler, arg);
        }

        public static Task<TResult> Aggregate<T1, T2, TResult>(this Func<T1, T2, Task<TResult>> func, T1 arg1, T2 arg2, TimeSpan? minimumInterval = null, TaskScheduler scheduler = null)
        {
            return Aggregate<TResult, Task<TResult>>(func, minimumInterval, scheduler, arg1, arg2);
        }

        public static Task<TResult> Aggregate<T1, T2, T3, TResult>(this Func<T1, T2, T3, Task<TResult>> func, T1 arg1, T2 arg2, T3 arg3, TimeSpan? minimumInterval = null, TaskScheduler scheduler = null)
        {
            return Aggregate<TResult, Task<TResult>>(func, minimumInterval, scheduler, arg1, arg2, arg3);
        }

        public static Task<TResult> Aggregate<T1, T2, T3, T4, TResult>(this Func<T1, T2, T3, T4, Task<TResult>> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4, TimeSpan? minimumInterval = null, TaskScheduler scheduler = null)
        {
            return Aggregate<TResult, Task<TResult>>(func, minimumInterval, scheduler, arg1, arg2, arg3, arg4);
        }

        private static TTask Aggregate<TResult, TTask>(MulticastDelegate @delegate, TimeSpan? minimumInterval = null, TaskScheduler scheduler = null, params object[] parameters) where TTask : Task
        {
            var delegateContext = DelegateContexts.GetOrAdd(@delegate, _ => new DelegateContext());
            lock (delegateContext)
            {
                // If there is already a task for this delegate, just return it.
                if (delegateContext.Task != null)
                    return (TTask)delegateContext.Task;

                // Get method info of the Invoke method of the delegate.
                var invokeMethodInfo = @delegate.GetType().GetMethod("Invoke", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.DeclaredOnly);

                var hasValidMinimumInterval = minimumInterval != null && minimumInterval > TimeSpan.Zero;
                if (hasValidMinimumInterval)
                {
                    // Check if the time between this invoke request and the previous one is too close.
                    var intervalFromLastInvoke = DateTime.Now - delegateContext.LastExecutionTime;
                    if (intervalFromLastInvoke < minimumInterval)
                    {
                        // If this invocation is too close to the previous one,
                        // wait until the minimum interval is passed,
                        // then invoke the delegate.
                        var waitTaskSource = new TaskCompletionSource<bool>();
                        delegateContext.Task = waitTaskSource.Task.ContinueWith(_ => (Task)invokeMethodInfo.Invoke(@delegate, parameters), scheduler).Unwrap();

                        // Force the wait in a background thread to ensure a non-blocking UI experience.
                        ThreadPool.QueueUserWorkItem(_ =>
                        {
                            Thread.Sleep(minimumInterval.Value - intervalFromLastInvoke);
                            waitTaskSource.SetResult(true);
                        });
                    }
                    else
                        // If this invocation is far enough from the last one,
                        // just invoke the delegate.
                        delegateContext.Task =
                            scheduler == null ?
                                (Task)invokeMethodInfo.Invoke(@delegate, parameters) :
                                Task.Factory.StartNew(() => (Task)invokeMethodInfo.Invoke(@delegate, parameters), CancellationToken.None, TaskCreationOptions.None, scheduler).Unwrap();
                }
                else
                    // If there is no valid minimum invocation interval,
                    // just invoke the delegate.
                    delegateContext.Task =
                        scheduler == null ?
                            (Task)invokeMethodInfo.Invoke(@delegate, parameters) :
                            Task.Factory.StartNew(() => (Task)invokeMethodInfo.Invoke(@delegate, parameters), CancellationToken.None, TaskCreationOptions.None, scheduler).Unwrap();

                // Define what happens when the task of the delegate completes.

                if (typeof(TTask) == typeof(Task<TResult>))
                    return (TTask)(delegateContext.Task = ((Task<TResult>)delegateContext.Task).ContinueWith(task =>
                    {
                        lock (delegateContext)
                            if (hasValidMinimumInterval)
                            {
                                // If there is a minimum invocation interval,
                                // just clear the task returned by the delegate,
                                // and update the execution time stamp.
                                delegateContext.Task = null;
                                delegateContext.LastExecutionTime = DateTime.Now;
                            }
                            else
                                // If there is no minimum invocation interval,
                                // just remove the function context.
                                DelegateContexts.TryRemove(@delegate, out delegateContext);
                        return task.Result;
                    }));

                return (TTask)(delegateContext.Task = delegateContext.Task.ContinueWith(task =>
                {
                    lock (delegateContext)
                        if (hasValidMinimumInterval)
                        {
                            // If there is a minimum invocation interval,
                            // just clear the task returned by the delegate,
                            // and update the execution time stamp.
                            delegateContext.Task = null;
                            delegateContext.LastExecutionTime = DateTime.Now;
                        }
                        else
                            // If there is no minimum invocation interval,
                            // just remove the function context.
                            DelegateContexts.TryRemove(@delegate, out delegateContext);
                }));
            }
        }

        private class DelegateContext
        {
            internal Task Task;
            internal DateTime LastExecutionTime;
        }
    }
}