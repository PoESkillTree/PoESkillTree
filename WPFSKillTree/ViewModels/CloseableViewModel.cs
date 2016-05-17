using System;
using System.Windows.Input;
using POESKillTree.Model;
using POESKillTree.Utils;

namespace POESKillTree.ViewModels
{
    /// <summary>
    /// Abstract class for ViewModels that can be closed.
    /// <para/>
    /// Handlers of <see cref="RequestClose"/> need to set <see cref="Result"/> to
    /// true or false if the View should be closed.
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
        /// Gets or sets the result of the ViewModel (null if it has none).
        /// </summary>
        public bool? Result
        {
            get { return _result; }
            protected set { SetProperty(ref _result, value); }
        }

        /// <summary>
        /// Raised when the close command is executed.
        /// </summary>
        protected event EventHandler RequestClose;

    }
}