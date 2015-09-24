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

        public ICommand CloseCommand
        {
            get { return _closeCommand ?? (_closeCommand = new RelayCommand(param => RequestClose.Raise(this))); }
        }

        private bool? _result;

        public bool? Result
        {
            get { return _result; }
            private set { SetProperty(ref _result, value); }
        }

        public event EventHandler RequestClose;

        protected void Close(bool? result)
        {
            Result = result;
            CloseCommand.Execute(null);
        }

    }
}