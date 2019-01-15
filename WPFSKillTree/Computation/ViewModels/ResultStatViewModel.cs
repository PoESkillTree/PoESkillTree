using System;
using System.Reactive.Concurrency;
using System.Windows.Input;
using PoESkillTree.Computation.Common;
using POESKillTree.Common.ViewModels;
using POESKillTree.Computation.Model;
using POESKillTree.Utils;

namespace POESKillTree.Computation.ViewModels
{
    public class ResultStatViewModel : Notifier, IDisposable
    {
        private IDisposable _subscription;

        public ResultStatViewModel(IStat stat, NodeType nodeType, Action<ResultStatViewModel> removeAction)
        {
            Node = new CalculationNodeViewModel(stat, nodeType);
            RemoveCommand = new RelayCommand(() => removeAction(this));
        }

        public CalculationNodeViewModel Node { get; }

        public ICommand RemoveCommand { get; }

        public void Observe(ObservableCalculator observableCalculator, IScheduler observeScheduler)
            => _subscription = Node.Observe(observableCalculator, observeScheduler);

        public void Dispose()
            => _subscription?.Dispose();
    }
}