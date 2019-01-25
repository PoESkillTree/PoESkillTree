using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using EnumsNET;
using PoESkillTree.Computation.Common;
using POESKillTree.Common.ViewModels;

namespace POESKillTree.Computation.ViewModels
{
    public class ResultStatsViewModel
    {
        private readonly CalculationNodeViewModelFactory _nodeFactory;
        private readonly ModifierNodeViewModelFactory _modifierNodeFactory;

        public ResultStatsViewModel(
            CalculationNodeViewModelFactory nodeFactory, ModifierNodeViewModelFactory modifierNodeFactory)
        {
            _nodeFactory = nodeFactory;
            _modifierNodeFactory = modifierNodeFactory;
            NewStat = new AddableResultStatViewModel();
            AddStatCommand = new RelayCommand(AddStat);
        }

        public ObservableCollection<ResultStatViewModel> Stats { get; } =
            new ObservableCollection<ResultStatViewModel>();

        public AddableResultStatViewModel NewStat { get; }

        public ObservableCollection<IStat> AvailableStats { get; } = new ObservableCollection<IStat>();
        public IEnumerable<NodeType> AvailableNodeTypes => Enums.GetValues<NodeType>();

        public ICommand AddStatCommand { get; }

        public void AddAvailableStat(IStat stat)
        {
            if (TryGetIndex(stat, out var index))
                return;

            AvailableStats.Insert(index, stat);
            if (index == 0)
            {
                NewStat.Stat = stat;
            }
        }

        /// <summary>
        /// Gets the index of the first <see cref="AvailableStats"/> that is ordered behind or at the same place as
        /// <see cref="stat"/>. The second means there is an entry that is equal to <see cref="stat"/>, in which case
        /// true is returned.
        /// </summary>
        private bool TryGetIndex(IStat stat, out int i)
        {
            for (i = 0; i < AvailableStats.Count; i++)
            {
                var availableStat = AvailableStats[i];
                if (availableStat.Entity < stat.Entity)
                    continue;
                if (availableStat.Entity > stat.Entity)
                    break;

                var stringComparision = string.Compare(availableStat.Identity, stat.Identity, StringComparison.Ordinal);
                if (stringComparision == 0)
                    return true;
                if (stringComparision > 0)
                    break;
            }
            return false;
        }

        private void AddStat()
            => AddStat(NewStat.Stat, NewStat.NodeType);

        public void AddStat(IStat stat, NodeType nodeType = NodeType.Total)
        {
            var resultStat =
                new ResultStatViewModel(_nodeFactory.CreateResult(stat, nodeType), _modifierNodeFactory, RemoveStat);
            Stats.Add(resultStat);
            AddAvailableStat(stat);
        }

        private void RemoveStat(ResultStatViewModel resultStat)
        {
            Stats.Remove(resultStat);
            NewStat.Stat = resultStat.Node.Stat;
            NewStat.NodeType = resultStat.Node.NodeType;
            resultStat.Dispose();
        }
    }
}