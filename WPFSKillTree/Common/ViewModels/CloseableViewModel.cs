using System.Threading.Tasks;
using System.Windows.Input;

namespace PoESkillTree.Common.ViewModels
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

        /// <summary>
        /// Returns true iff the close command can currently be executed.
        /// This implementation always returns true.
        /// </summary>
        protected virtual bool CanClose()
        {
            return true;
        }

        protected sealed override bool CanClose(object param)
        {
            return CanClose();
        }

        /// <summary>
        /// Called when CloseCommand is executed. Default implementation does nothing.
        /// </summary>
        protected virtual void OnClose()
        {
        }

        protected sealed override void OnClose(object param)
        {
            OnClose();
            base.OnClose(param);
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

        private readonly TaskCompletionSource<T> _closeCompletionSource =
            new TaskCompletionSource<T>();

        protected CloseableViewModel()
        {
            CloseCommand = new RelayCommand<T>(OnClose, CanClose);
        }

        /// <summary>
        /// Returns a task that completes with the produced result once <see cref="CloseCommand"/>
        /// is executed.
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
        /// This implementation always returns true.
        /// </summary>
        protected virtual bool CanClose(T param)
        {
            return true;
        }

        /// <summary>
        /// Called when <see cref="CloseCommand"/> is executed. Default implementation completes the closing task and
        /// must be called by overriding methods.
        /// </summary>
        protected virtual void OnClose(T param)
        {
            _closeCompletionSource.TrySetResult(param);
        }
    }
}