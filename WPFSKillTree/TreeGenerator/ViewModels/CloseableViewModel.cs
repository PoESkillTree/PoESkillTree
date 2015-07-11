using System;
using System.Windows.Input;
using POESKillTree.Model;

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
            get
            {
                if (_closeCommand == null)
                {
                    _closeCommand = new RelayCommand(param => OnRequestClose());
                }
                return _closeCommand;
            }
        }

        private bool? _result;

        public bool? Result
        {
            get { return _result; }
            private set
            {
                _result = value;
                OnPropertyChanged("Result");
            }
        }

        public event EventHandler RequestClose;

        private void OnRequestClose()
        {
            var handler = RequestClose;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        protected void Close(bool? result)
        {
            Result = result;
            CloseCommand.Execute(null);
        }

    }
}