using System;
using System.Threading.Tasks;

namespace POESKillTree.Utils.Extensions
{
    public static class ObservableExtensions
    {
        public static Task SubscribeAndAwaitCompletionAsync<T>(this IObservable<T> @this, Action<T> onNext)
        {
            var taskCompletionSource = new TaskCompletionSource<object>();
            @this.Subscribe(onNext,
                taskCompletionSource.SetException,
                () => taskCompletionSource.SetResult(new object()));
            return taskCompletionSource.Task;
        }
    }
}