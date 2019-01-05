using System;
using System.Threading.Tasks;

namespace POESKillTree.Utils
{
    public static class ObservableExtensions
    {
        public static async Task SubscribeAndAwaitCompletionAsync<T>(this IObservable<T> @this, Action<T> onNext)
        {
            var taskCompletionSource = new TaskCompletionSource<object>();
            @this.Subscribe(onNext,
                taskCompletionSource.SetException,
                () => taskCompletionSource.SetResult(new object()));
            await taskCompletionSource.Task;
        }
    }
}