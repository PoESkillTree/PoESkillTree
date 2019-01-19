using System;
using System.Windows.Input;
using POESKillTree.Common.ViewModels;

namespace POESKillTree.Computation.ViewModels
{
    public class ResultStatViewModel
    {
        public ResultStatViewModel(CalculationNodeViewModel node, Action<ResultStatViewModel> removeAction)
        {
            Node = node;
            RemoveCommand = new RelayCommand(() => removeAction(this));
        }

        public CalculationNodeViewModel Node { get; }

        public ICommand RemoveCommand { get; }
    }
}