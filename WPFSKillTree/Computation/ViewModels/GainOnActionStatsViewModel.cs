using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Concurrency;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core;
using POESKillTree.Computation.Model;

namespace POESKillTree.Computation.ViewModels
{
    public class GainOnActionStatsViewModel
    {
        private readonly ExplicitlyRegisteredStatsObserver _explicitlyRegisteredStats;
        private readonly CalculationNodeViewModelFactory _nodeFactory;

        private GainOnActionStatsViewModel(
            CalculationNodeViewModelFactory nodeFactory, ExplicitlyRegisteredStatsObserver explicitlyRegisteredStats)
        {
            _nodeFactory = nodeFactory;
            _explicitlyRegisteredStats = explicitlyRegisteredStats;
        }

        public static GainOnActionStatsViewModel Create(
            ObservableCalculator observableCalculator, CalculationNodeViewModelFactory nodeFactory)
        {
            var explicitlyRegisteredStats = new ExplicitlyRegisteredStatsObserver(observableCalculator);
            var vm = new GainOnActionStatsViewModel(nodeFactory, explicitlyRegisteredStats);
            vm.Initialize();
            return vm;
        }

        private void Initialize()
        {
            _explicitlyRegisteredStats.StatAdded += (node, stat) => DoIfResponsible(node, stat, Add);
            _explicitlyRegisteredStats.StatRemoved += (node, stat) => DoIfResponsible(node, stat, Remove);
            _explicitlyRegisteredStats.Initialize(DispatcherScheduler.Current);
        }

        public ObservableCollection<GainOnActionStatViewModel> Stats { get; } =
            new ObservableCollection<GainOnActionStatViewModel>();

        private static void DoIfResponsible(ICalculationNode node, IStat stat, Action<ICalculationNode, IStat> action)
        {
            if (IsResponsibleFor(stat))
                action(node, stat);
        }

        private static bool IsResponsibleFor(IStat stat)
            => stat.ExplicitRegistrationType is ExplicitRegistrationType.GainOnAction;

        private void Add(ICalculationNode node, IStat stat)
        {
            if (TryGetStatViewModel(stat, out _))
                return;

            var statVm = new GainOnActionStatViewModel(_nodeFactory.CreateResult(stat, node));
            Stats.Add(statVm);
        }

        private void Remove(ICalculationNode node, IStat stat)
        {
            if (!TryGetStatViewModel(stat, out var statVm))
                return;

            Stats.Remove(statVm);
            statVm.Dispose();
        }

        private bool TryGetStatViewModel(IStat stat, out GainOnActionStatViewModel statVm)
        {
            statVm = Stats.FirstOrDefault(s => s.Node.Stat.Equals(stat));
            return statVm != null;
        }
    }
}