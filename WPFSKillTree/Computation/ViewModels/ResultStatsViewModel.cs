using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using EnumsNET;
using PoESkillTree.Computation.Common;
using POESKillTree.Common.ViewModels;

namespace POESKillTree.Computation.ViewModels
{
    public class ResultStatsViewModel
    {
        public ResultStatsViewModel()
        {
            NewStat = new AddableResultStatViewModel();
            AddStatCommand = new RelayCommand(AddStat);
        }

        public ObservableCollection<ResultStatViewModel> Stats { get; } =
            new ObservableCollection<ResultStatViewModel>();

        public AddableResultStatViewModel NewStat { get; }

        public ObservableCollection<IStat> AvailableStats { get; } = new ObservableCollection<IStat>();
        public IEnumerable<NodeType> AvailableNodeTypes => Enums.GetValues<NodeType>();

        public ICommand AddStatCommand { get; }

        public void AddStat(IStat stat, NodeValue? value = null, NodeType nodeType = NodeType.Total)
        {
            var newStat = new ResultStatViewModel(stat, nodeType, RemoveStat)
                { Value = value };
            Stats.Add(newStat);
        }

        public void AddAvailableStat(IStat stat)
        {
            if (Stats.Any(s => s.Stat.Equals(stat)))
                return;

            if (NewStat.Stat is null)
            {
                NewStat.Stat = stat;
            }
            AvailableStats.Add(stat);
        }

        private void AddStat()
        {
            var newStat = new ResultStatViewModel(NewStat.Stat, NewStat.NodeType, RemoveStat)
                { Value = new NodeValue(NewStat.Stat.GetHashCode()) };
            Stats.Add(newStat);
            AvailableStats.Remove(NewStat.Stat);
        }

        private void RemoveStat(ResultStatViewModel resultStat)
        {
            Stats.Remove(resultStat);
            var stat = resultStat.Stat;
            AvailableStats.Add(stat);
            NewStat.Stat = stat;
            NewStat.NodeType = resultStat.NodeType;
        }
    }
}