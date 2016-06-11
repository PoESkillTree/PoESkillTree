using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace POESKillTree.Utils
{
    // Original source: Stephen Cleary at https://msdn.microsoft.com/en-us/magazine/dn605875.aspx
    /// <summary>
    /// Wrapper class around Task that notifies property changes, has
    /// a non-blocking <see cref="Result"/> property and swallows exceptions
    /// in case no exception handler is provided.
    /// </summary>
    public sealed class NotifyingTask<TResult> : INotifyPropertyChanged
    {
        public TResult Default { private get; set; }
        public Task TaskCompletion { get; private set; }
        public Task<TResult> Task { get; private set; }
        public TResult Result
        {
            get { return (Task.Status == TaskStatus.RanToCompletion) ? Task.Result : Default; }
        }
        public TaskStatus Status { get { return Task.Status; } }
        public bool IsCompleted { get { return Task.IsCompleted; } }
        public bool IsNotCompleted { get { return !Task.IsCompleted; } }
        public bool IsSuccessfullyCompleted
        {
            get { return Task.Status == TaskStatus.RanToCompletion; }
        }
        public bool IsCanceled { get { return Task.IsCanceled; } }
        public bool IsFaulted { get { return Task.IsFaulted; } }
        public AggregateException Exception { get { return Task.Exception; } }
        public Exception InnerException
        {
            get
            {
                return (Exception == null) ?
                    null : Exception.InnerException;
            }
        }
        public string ErrorMessage
        {
            get
            {
                return (InnerException == null) ?
                    null : InnerException.Message;
            }
        }

        public NotifyingTask(Task<TResult> task, Action<Exception> errorHandler)
        {
            Task = task;
            if (!task.IsCompleted)
            {
                TaskCompletion = WatchTaskAsync(task, errorHandler, null);
            }
        }

        public NotifyingTask(Task<TResult> task, Func<Exception, Task> errorHandler = null)
        {
            Task = task;
            if (!task.IsCompleted)
            {
                TaskCompletion = WatchTaskAsync(task, null, errorHandler);
            }
        }

        private async Task WatchTaskAsync(Task task, Action<Exception> errorHandler, Func<Exception, Task> asyncErrorHandler)
        {
            Exception e = null;
            try
            {
                await task;
            }
            catch (Exception e1)
            {
                // No await in catch with C# 5.0
                e = e1;
            }
            if (e != null)
            {
                if (errorHandler != null)
                    errorHandler(e);
                if (asyncErrorHandler != null)
                    await asyncErrorHandler(e);
            }

            var propertyChanged = PropertyChanged;
            if (propertyChanged == null)
                return;
            propertyChanged(this, new PropertyChangedEventArgs("Status"));
            propertyChanged(this, new PropertyChangedEventArgs("IsCompleted"));
            propertyChanged(this, new PropertyChangedEventArgs("IsNotCompleted"));
            if (task.IsCanceled)
            {
                propertyChanged(this, new PropertyChangedEventArgs("IsCanceled"));
            }
            else if (task.IsFaulted)
            {
                propertyChanged(this, new PropertyChangedEventArgs("IsFaulted"));
                propertyChanged(this, new PropertyChangedEventArgs("Exception"));
                propertyChanged(this,
                    new PropertyChangedEventArgs("InnerException"));
                propertyChanged(this, new PropertyChangedEventArgs("ErrorMessage"));
            }
            else
            {
                propertyChanged(this,
                    new PropertyChangedEventArgs("IsSuccessfullyCompleted"));
                propertyChanged(this, new PropertyChangedEventArgs("Result"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}