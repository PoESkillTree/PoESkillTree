using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using EnumsNET;
using PoESkillTree.Computation.Common;
using POESKillTree.Common.ViewModels;
using POESKillTree.Computation.Model;

namespace POESKillTree.Computation.ViewModels
{
    public class ResultStatsViewModel
    {
        private readonly ObservableCalculator _observableCalculator;

        public ResultStatsViewModel(ObservableCalculator observableCalculator)
        {
            _observableCalculator = observableCalculator;
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
            if (Stats.Any(s => s.Stat.Equals(stat)))
                return;

            if (NewStat.Stat is null)
            {
                NewStat.Stat = stat;
            }
            AvailableStats.Add(stat);
        }

        private void AddStat()
            => AddStat(NewStat.Stat, NewStat.NodeType);

        public void AddStat(IStat stat, NodeType nodeType = NodeType.Total)
        {
            var newStat = new ResultStatViewModel(stat, nodeType, RemoveStat);
            newStat.Connect(_observableCalculator);
            Stats.Add(newStat);
            AvailableStats.Remove(stat);
            NewStat.Stat = AvailableStats.FirstOrDefault();
        }

        private void RemoveStat(ResultStatViewModel resultStat)
        {
            Stats.Remove(resultStat);
            AvailableStats.Add(resultStat.Stat);
            NewStat.Stat = resultStat.Stat;
            NewStat.NodeType = resultStat.NodeType;
            resultStat.Dispose();
        }
    }
}