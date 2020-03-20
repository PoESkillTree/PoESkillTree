using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace PoESkillTree.Utils
{
    public static class NotifyingTask
    {
        public static NotifyingTask<TResult> Create<TResult>(Task<TResult> task, Action<Exception> errorHandler)
            where TResult: struct =>
            new NotifyingTask<TResult>(task, default, errorHandler);

        public static NotifyingTask<TResult> WithDefaultResult<TResult>()
            where TResult: struct =>
            FromResult(default(TResult));

        public static NotifyingTask<TResult> FromResult<TResult>(TResult result) =>
            new NotifyingTask<TResult>(Task.FromResult(result), result, _ => { });
    }

    // Original source: Stephen Cleary at https://msdn.microsoft.com/en-us/magazine/dn605875.aspx
    /// <summary>
    /// Wrapper class around Task that notifies property changes, has
    /// a non-blocking <see cref="Result"/> property.
    /// </summary>
    public sealed class NotifyingTask<TResult> : INotifyPropertyChanged
    {
        private readonly TResult _defaultResult;
        public Task TaskCompletion { get; }
        public Task<TResult> Task { get; }
        public TResult Result => IsSuccessfullyCompleted ? Task.Result : _defaultResult;
        public TaskStatus Status => Task.Status;
        public bool IsCompleted => Task.IsCompleted;
        public bool IsNotCompleted => !Task.IsCompleted;
        public bool IsSuccessfullyCompleted => Task.Status == TaskStatus.RanToCompletion;
        public bool IsCanceled => Task.IsCanceled;
        public bool IsFaulted => Task.IsFaulted;
        public AggregateException? Exception => Task.Exception;
        public Exception? InnerException => Exception?.InnerException;
        public string? ErrorMessage => InnerException?.Message;

        private readonly Func<Exception, Task> _errorHandler;

        public NotifyingTask(Task<TResult> task, TResult defaultResult, Action<Exception> errorHandler)
            : this(task, defaultResult, e =>
            {
                errorHandler(e);
                return System.Threading.Tasks.Task.CompletedTask;
            })
        {
        }

        public NotifyingTask(Task<TResult> task, TResult defaultResult, Func<Exception, Task> errorHandler)
        {
            Task = task;
            _defaultResult = defaultResult;
            _errorHandler = errorHandler;
            TaskCompletion = task.IsCompleted ? System.Threading.Tasks.Task.CompletedTask : WatchTaskAsync();
        }

        private async Task WatchTaskAsync()
        {
            try
            {
                await Task;
            }
            catch (Exception e)
            {
                await _errorHandler(e);
            }

            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(IsCompleted));
            OnPropertyChanged(nameof(IsNotCompleted));
            if (Task.IsCanceled)
            {
                OnPropertyChanged(nameof(IsCanceled));
            }
            else if (Task.IsFaulted)
            {
                OnPropertyChanged(nameof(IsFaulted));
                OnPropertyChanged(nameof(Exception));
                OnPropertyChanged(nameof(InnerException));
                OnPropertyChanged(nameof(ErrorMessage));
            }
            else
            {
                OnPropertyChanged(nameof(IsSuccessfullyCompleted));
                OnPropertyChanged(nameof(Result));
            }
        }

        private void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public event PropertyChangedEventHandler? PropertyChanged;

        public NotifyingTask<TResult> Select(Func<TResult, TResult> selector) =>
            new NotifyingTask<TResult>(SelectAsync(selector), _defaultResult, _errorHandler);

        private async Task<TResult> SelectAsync(Func<TResult, TResult> selector) =>
            selector(await Task);
    }
}