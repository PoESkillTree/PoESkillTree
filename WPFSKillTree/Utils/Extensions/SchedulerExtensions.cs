using System;
using System.Reactive.Concurrency;
using System.Threading.Tasks;

namespace POESKillTree.Utils.Extensions
{
    public static class SchedulerExtensions
    {
        public static Task<T> ScheduleAsync<T>(this IScheduler @this, Func<T> action)
        {
            var taskCompletionSource = new TaskCompletionSource<T>();
            @this.Schedule(() =>
            {
                try
                {
                    var node = action();
                    taskCompletionSource.SetResult(node);
                }
                catch (Exception e)
                {
                    taskCompletionSource.SetException(e);
                }
            });
            return taskCompletionSource.Task;
        }
    }
}