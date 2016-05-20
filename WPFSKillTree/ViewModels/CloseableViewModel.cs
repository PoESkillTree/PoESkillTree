using System;
using System.Windows.Input;
using POESKillTree.Model;

namespace POESKillTree.ViewModels
{
    /// <summary>
    /// Abstract class for ViewModels that can be closed.
    /// </summary>
    public abstract class CloseableViewModel : ViewModelBase
    {
        private readonly RelayCommand _closeCommand;
        /// <summary>
        /// Gets the command that closes this ViewModel.
        /// </summary>
        public ICommand CloseCommand
        {
            get { return _closeCommand; }
        }

        /// <summary>
        /// Raised when <see cref="CloseCommand"/> is executed.
        /// </summary>
        public event Action RequestsClose;

        protected CloseableViewModel()
        {
            _closeCommand = new RelayCommand(x =>
            {
                if (RequestsClose != null)
                    RequestsClose();
            });
        }
    }
}