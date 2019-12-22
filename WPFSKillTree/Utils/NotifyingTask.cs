using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace PoESkillTree.Utils
{
    // Original source: Stephen Cleary at https://msdn.microsoft.com/en-us/magazine/dn605875.aspx
    /// <summary>
    /// Wrapper class around Task that notifies property changes, has
    /// a non-blocking <see cref="Result"/> property.
    /// </summary>
    public sealed class NotifyingTask<TResult> : INotifyPropertyChanged
    {
        [MaybeNull]
        public TResult Default { private get; set; } = default!;
        public Task TaskCompletion { get; }
        public Task<TResult> Task { get; }
        public TResult Result => IsSuccessfullyCompleted ? Task.Result : Default;
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

        public NotifyingTask(Task<TResult> task, Action<Exception> errorHandler)
            : this(task, e =>
            {
                errorHandler(e);
                return System.Threading.Tasks.Task.CompletedTask;
            })
        {
        }

        public NotifyingTask(Task<TResult> task, Func<Exception, Task> errorHandler)
        {
            Task = task;
            _errorHandler = errorHandler;
            TaskCompletion = task.IsCompleted ? System.Threading.Tasks.Task.CompletedTask : WatchTaskAsync();
        }

        public static NotifyingTask<TResult> FromResult(TResult result) =>
            new NotifyingTask<TResult>(System.Threading.Tasks.Task.FromResult(result), _ => { });

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
            new NotifyingTask<TResult>(SelectAsync(selector), _errorHandler) {Default = Default};

        private async Task<TResult> SelectAsync(Func<TResult, TResult> selector) =>
            selector(await Task);
    }
}