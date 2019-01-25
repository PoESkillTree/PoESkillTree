using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace POESKillTree.Utils
{
    // Original source: Stephen Cleary at https://msdn.microsoft.com/en-us/magazine/dn605875.aspx
    /// <summary>
    /// Wrapper class around Task that notifies property changes, has
    /// a non-blocking <see cref="Result"/> property.
    /// </summary>
    public sealed class NotifyingTask<TResult> : INotifyPropertyChanged
    {
        public TResult Default { private get; set; }
        public Task TaskCompletion { get; }
        public Task<TResult> Task { get; }
        public TResult Result => (Task.Status == TaskStatus.RanToCompletion) ? Task.Result : Default;
        public TaskStatus Status => Task.Status;
        public bool IsCompleted => Task.IsCompleted;
        public bool IsNotCompleted => !Task.IsCompleted;
        public bool IsSuccessfullyCompleted => Task.Status == TaskStatus.RanToCompletion;
        public bool IsCanceled => Task.IsCanceled;
        public bool IsFaulted => Task.IsFaulted;
        public AggregateException Exception => Task.Exception;
        public Exception InnerException => Exception?.InnerException;
        public string ErrorMessage => InnerException?.Message;

        public NotifyingTask(Task<TResult> task, Action<Exception> errorHandler)
        {
            Task = task;
            if (!task.IsCompleted)
            {
                TaskCompletion = WatchTaskAsync(task, errorHandler, null);
            }
        }

        public NotifyingTask(Task<TResult> task, Func<Exception, Task> errorHandler)
        {
            Task = task;
            if (!task.IsCompleted)
            {
                TaskCompletion = WatchTaskAsync(task, null, errorHandler);
            }
        }

        private async Task WatchTaskAsync(Task task, Action<Exception> errorHandler, Func<Exception, Task> asyncErrorHandler)
        {
            try
            {
                await task;
            }
            catch (Exception e)
            {
                errorHandler?.Invoke(e);
                if (asyncErrorHandler != null)
                    await asyncErrorHandler(e);
            }

            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(IsCompleted));
            OnPropertyChanged(nameof(IsNotCompleted));
            if (task.IsCanceled)
            {
                OnPropertyChanged(nameof(IsCanceled));
            }
            else if (task.IsFaulted)
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

        public event PropertyChangedEventHandler PropertyChanged;
    }
}