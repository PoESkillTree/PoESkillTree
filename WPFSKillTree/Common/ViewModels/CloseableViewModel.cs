using System.Threading.Tasks;
using System.Windows.Input;

namespace PoESkillTree.Common.ViewModels
{
    /// <summary>
    /// Base class for ViewModels that can be closed.
    /// Can be used directly for views that don't need a view model.
    /// </summary>
    public class CloseableViewModel : CloseableViewModelBase<object?>
    {
        public CloseableViewModel()
        {
            CloseCommand = new RelayCommand(() => Close(null), () => true);
        }

        public override ICommand CloseCommand { get; }

        protected sealed override void OnClose(object? param)
        {
            OnClose();
        }

        /// <summary>
        /// Called when CloseCommand is executed. Default implementation does nothing.
        /// </summary>
        protected virtual void OnClose()
        {
        }
    }

    /// <summary>
    /// Base class for ViewModels that can be closed and have a result.
    /// Can be used directly for views that don't need a view model.
    /// </summary>
    /// <typeparam name="T">The type of the result this dialog produces.</typeparam>
    public abstract class CloseableViewModel<T> : CloseableViewModelBase<T>
    {
        protected CloseableViewModel()
        {
            CloseCommand = new RelayCommand<T>(Close, CanClose, true);
        }

        public override ICommand CloseCommand { get; }

        /// <summary>
        /// Returns true iff the close command can currently be executed with the given parameter.
        /// This implementation always returns true.
        /// </summary>
        protected virtual bool CanClose(T param)
            => true;
    }

    public abstract class CloseableViewModelBase<T> : ViewModelBase
    {
        /// <summary>
        /// Gets the command that closes this ViewModel.
        /// </summary>
        public abstract ICommand CloseCommand { get; }

        private readonly TaskCompletionSource<T> _closeCompletionSource =
            new TaskCompletionSource<T>();

        /// <summary>
        /// Returns a task that completes with the produced result once <see cref="CloseCommand"/>
        /// is executed.
        /// </summary>
        public Task<T> WaitForCloseAsync()
            => _closeCompletionSource.Task;

        protected void Close(T param)
        {
            OnClose(param);
            _closeCompletionSource.TrySetResult(param);
        }
        
        /// <summary>
        /// Called when <see cref="CloseCommand"/> is executed. Default implementation does nothing.
        /// </summary>
        protected virtual void OnClose(T param)
        {
        }
    }
}