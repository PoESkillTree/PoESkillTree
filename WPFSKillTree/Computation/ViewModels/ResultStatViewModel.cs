using System;
using System.Windows.Input;
using POESKillTree.Common.ViewModels;

namespace POESKillTree.Computation.ViewModels
{
    public class ResultStatViewModel
    {
        public ResultStatViewModel(ResultNodeViewModel node, Action<ResultStatViewModel> removeAction)
        {
            Node = node;
            RemoveCommand = new RelayCommand(() => removeAction(this));
        }

        public ResultNodeViewModel Node { get; }

        public ICommand RemoveCommand { get; }
    }
}