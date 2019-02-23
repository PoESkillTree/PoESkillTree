using System;
using System.Reactive.Concurrency;
using System.Threading.Tasks;

namespace POESKillTree.Utils.Extensions
{
    public static class SchedulerExtensions
    {
        public static Task ScheduleAsync(this IScheduler @this, Action action)
            => @this.ScheduleAsync(() => { action(); return Task.FromResult(default(object)); });

        public static Task<T> ScheduleAsync<T>(this IScheduler @this, Func<T> action)
            => @this.ScheduleAsync(() => Task.FromResult(action()));

        public static Task<T> ScheduleAsync<T>(this IScheduler @this, Func<Task<T>> action)
        {
            var taskCompletionSource = new TaskCompletionSource<T>();
            @this.ScheduleAsync(async (_, __) =>
            {
                try
                {
                    var result = await action().ConfigureAwait(false);
                    taskCompletionSource.SetResult(result);
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