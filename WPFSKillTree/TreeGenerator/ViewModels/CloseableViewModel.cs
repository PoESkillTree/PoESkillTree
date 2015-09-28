using System;
using System.Windows.Input;
using POESKillTree.Model;
using POESKillTree.Utils;

namespace POESKillTree.TreeGenerator.ViewModels
{
    /// <summary>
    /// Abstract class for ViewModels that can be closed.
    /// </summary>
    public abstract class CloseableViewModel : ViewModelBase
    {

        private RelayCommand _closeCommand;
        /// <summary>
        /// Gets the command that closes this ViewModel.
        /// </summary>
        public ICommand CloseCommand
        {
            get { return _closeCommand ?? (_closeCommand = new RelayCommand(param => RequestClose.Raise(this))); }
        }

        private bool? _result;
        /// <summary>
        /// Gets the result of the ViewModel or null if it has none.
        /// </summary>
        public bool? Result
        {
            get { return _result; }
            private set { SetProperty(ref _result, value); }
        }

        /// <summary>
        /// Raised when the close command is executed.
        /// </summary>
        public event EventHandler RequestClose;

        /// <summary>
        /// Executes the close command and sets the given value as result.
        /// </summary>
        protected void Close(bool? result)
        {
            Result = result;
            CloseCommand.Execute(null);
        }

    }
}