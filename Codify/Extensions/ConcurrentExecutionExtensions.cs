using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Codify.Windows.Extensions
{
    /// <summary>
    /// A collection of extension methods to aggregate calls to a specified asynchronous function that returns a <see cref="Task{TResult}"/>.
    /// 
    /// If the function is called while the asynchronous process of the function is still in progress, the same <see cref="Task{TResult}"/>
    /// will be returned to the caller.
    /// 
    /// If the function is called with a specified minimum interval, and the time interval between this call and the completion of the last call
    /// is shorter then the minimum interval, a new <see cref="Task{TResult}"/> is generated and returned immediately, this task then waits until
    /// the minimum interval is elapsed before executing the actual function.
    /// 
    /// This extension supports function delegates taking up to 8 parameters.
    /// </summary>
    public static class ConcurrentExecutionExtensions
    {
        private static readonly ConcurrentDictionary<MulticastDelegate, FunctionContext> FunctionContexts = new ConcurrentDictionary<MulticastDelegate, FunctionContext>();

        public static Task<TResult> Aggregate<T, TResult>(this Func<T, Task<TResult>> func, T arg, TimeSpan? minimumInterval = null)
        {
            return Aggregate<TResult>(func, minimumInterval, arg);
        }

        public static Task<TResult> Aggregate<T1, T2, TResult>(this Func<T1, T2, Task<TResult>> func, T1 arg1, T2 arg2, TimeSpan? minimumInterval = null)
        {
            return Aggregate<TResult>(func, minimumInterval, arg1, arg2);
        }

        public static Task<TResult> Aggregate<T1, T2, T3, TResult>(this Func<T1, T2, T3, Task<TResult>> func, T1 arg1, T2 arg2, T3 arg3, TimeSpan? minimumInterval = null)
        {
            return Aggregate<TResult>(func, minimumInterval, arg1, arg2, arg3);
        }

        public static Task<TResult> Aggregate<T1, T2, T3, T4, TResult>(this Func<T1, T2, T3, T4, Task<TResult>> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4, TimeSpan? minimumInterval = null)
        {
            return Aggregate<TResult>(func, minimumInterval, arg1, arg2, arg3, arg4);
        }

        public static Task<TResult> Aggregate<T1, T2, T3, T4, T5, TResult>(this Func<T1, T2, T3, T4, T5, Task<TResult>> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, TimeSpan? minimumInterval = null)
        {
            return Aggregate<TResult>(func, minimumInterval, arg1, arg2, arg3, arg4, arg5);
        }

        public static Task<TResult> Aggregate<T1, T2, T3, T4, T5, T6, TResult>(this Func<T1, T2, T3, T4, T5, T6, Task<TResult>> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, TimeSpan? minimumInterval = null)
        {
            return Aggregate<TResult>(func, minimumInterval, arg1, arg2, arg3, arg4, arg5, arg6);
        }

        public static Task<TResult> Aggregate<T1, T2, T3, T4, T5, T6, T7, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, Task<TResult>> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, TimeSpan? minimumInterval = null)
        {
            return Aggregate<TResult>(func, minimumInterval, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }

        public static Task<TResult> Aggregate<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, Task<TResult>> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, TimeSpan? minimumInterval = null)
        {
            return Aggregate<TResult>(func, minimumInterval, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }

        private static Task<TResult> Aggregate<TResult>(this MulticastDelegate func, TimeSpan? minimumInterval = null, params object[] parameters)
        {
            var functionContext = FunctionContexts.GetOrAdd(func, _ => new FunctionContext());
            lock (functionContext)
            {
                // If there is already a download task, just return this one.
                if (functionContext.Task != null)
                    return (Task<TResult>)functionContext.Task;

                // Get method info of the Invoke method of the function delegate.
                var invokeMethodInfo = func.GetType().GetMethod("Invoke", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.DeclaredOnly);

                var hasValidMinimumInterval = minimumInterval != null && minimumInterval > TimeSpan.Zero;
                if (hasValidMinimumInterval)
                {
                    // Check if the time between this invoke request and the previous one is too close.
                    var intervalFromLastInvoke = DateTime.Now - functionContext.LastExecutionTime;
                    if (intervalFromLastInvoke < minimumInterval)
                    {
                        // If this invoke request is too close to the previous one,
                        // make this request wait until the minimum interval is passed,
                        // then invoke the function.
                        var waitTaskSource = new TaskCompletionSource<bool>();
                        functionContext.Task = waitTaskSource.Task.ContinueWith(_ => (Task<TResult>)invokeMethodInfo.Invoke(func, parameters)).Unwrap();

                        // Force the wait in a background thread to ensure a non-blocking UI experience.
                        ThreadPool.QueueUserWorkItem(_ =>
                        {
                            Thread.Sleep(minimumInterval.Value - intervalFromLastInvoke);
                            waitTaskSource.SetResult(true);
                        });
                    }
                    else
                        // If this invoke request is far enough from the last one,
                        // just invoke the function.
                        functionContext.Task = (Task)invokeMethodInfo.Invoke(func, parameters);
                }
                else
                    // If there is no valid minimum invocation interval,
                    // just invoke the function.
                    functionContext.Task = (Task)invokeMethodInfo.Invoke(func, parameters);

                // When the function task is completed, always clear the task reference
                // and update the invoke timestamp.
                return (Task<TResult>)(functionContext.Task = ((Task<TResult>)functionContext.Task).ContinueWith(task =>
                {
                    lock (functionContext)
                    {
                        if (hasValidMinimumInterval)
                        {
                            // If a valid minimum invocation interval is specified,
                            // just clear the task returned by the function,
                            // and update the execution time stamp.
                            functionContext.Task = null;
                            functionContext.LastExecutionTime = DateTime.Now;
                        }
                        else
                            // If no valid minimum invocation interval is specified,
                            // just remove the function context.
                            FunctionContexts.TryRemove(func, out functionContext);
                        return task.Result;
                    }
                }));
            }
        }

        private class FunctionContext
        {
            internal Task Task;
            internal DateTime LastExecutionTime;
        }
    }
}