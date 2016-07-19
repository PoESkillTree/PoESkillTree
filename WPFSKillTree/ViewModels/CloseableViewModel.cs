using System;
using System.Threading.Tasks;
using System.Windows.Input;
using POESKillTree.Model;

namespace POESKillTree.ViewModels
{
    /// <summary>
    /// Base class for ViewModels that can be closed.
    /// Can be used directly for views that don't need a view model.
    /// </summary>
    public class CloseableViewModel : CloseableViewModel<object>
    {
        protected void Close()
        {
            Close(null);
        }

        protected virtual bool CanClose()
        {
            return true;
        }

        protected override bool CanClose(object param)
        {
            return CanClose();
        }
    }

    /// <summary>
    /// Base class for ViewModels that can be closed and have a result.
    /// Can be used directly for views that don't need a view model.
    /// </summary>
    /// <typeparam name="T">The type of the result this dialog produces.</typeparam>
    public class CloseableViewModel<T> : ViewModelBase
    {
        /// <summary>
        /// Gets the command that closes this ViewModel.
        /// </summary>
        public ICommand CloseCommand { get; }

        /// <summary>
        /// Raised when <see cref="CloseCommand"/> is executed.
        /// </summary>
        public event Action<T> RequestsClose;

        private readonly TaskCompletionSource<T> _closeCompletionSource =
            new TaskCompletionSource<T>();

        protected CloseableViewModel()
        {
            CloseCommand = new RelayCommand<T>(param =>
            {
                RequestsClose?.Invoke(param);
                _closeCompletionSource.TrySetResult(param);
            }, CanClose);
        }

        /// <summary>
        /// Returns a task that completes with the produced result once <see cref="CloseCommand"/>
        /// is executed and all handlers for <see cref="RequestsClose"/> returned.
        /// </summary>
        public Task<T> WaitForCloseAsync()
        {
            return _closeCompletionSource.Task;
        }

        /// <summary>
        /// Closes this ViewModel. Equivalent to executing <see cref="CloseCommand"/> if it can currently be executed.
        /// </summary>
        protected void Close(T param)
        {
            if (CloseCommand.CanExecute(param))
                CloseCommand.Execute(param);
        }

        /// <summary>
        /// Returns true iff the close command can currently be executed with the given parameter.
        /// </summary>
        protected virtual bool CanClose(T param)
        {
            return true;
        }
    }
}