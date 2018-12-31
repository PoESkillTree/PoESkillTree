using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using EnumsNET;
using PoESkillTree.Computation.Common;
using PoESkillTree.GameModel;
using POESKillTree.Common.ViewModels;

namespace POESKillTree.Computation.ViewModels
{
    public class ResultStatsViewModel
    {
        public ResultStatsViewModel(ObservableCollection<string> availableStatIdentities)
        {
            AvailableStatIdentities = availableStatIdentities;
            NewStat = new AddableResultStatViewModel { Identity = AvailableStatIdentities[0] };
            AddStatCommand = new RelayCommand(AddStat);
        }

        public ObservableCollection<ResultStatViewModel> Stats { get; } =
            new ObservableCollection<ResultStatViewModel>();

        public AddableResultStatViewModel NewStat { get; }

        public ObservableCollection<string> AvailableStatIdentities { get; }
        public IEnumerable<Entity> AvailableEntities => Enums.GetValues<Entity>();
        public IEnumerable<NodeType> AvailableNodeTypes => Enums.GetValues<NodeType>();

        public ICommand AddStatCommand { get; }

        public void AddStat(IStat stat, NodeValue? value = null, NodeType nodeType = NodeType.Total)
        {
            var newStat = new ResultStatViewModel(stat, nodeType, RemoveStat)
                { Value = value };
            Stats.Add(newStat);
        }

        private void AddStat()
        {
            var newStat = new ResultStatViewModel(new StatStub(NewStat.Identity, NewStat.Entity, typeof(double)),
                    NewStat.NodeType, RemoveStat)
                { Value = new NodeValue(NewStat.Identity.GetHashCode()) };
            Stats.Add(newStat);
            AvailableStatIdentities.Remove(NewStat.Identity);
        }

        private void RemoveStat(ResultStatViewModel resultStat)
        {
            Stats.Remove(resultStat);
            var stat = resultStat.Stat;
            AvailableStatIdentities.Add(stat.Identity);
            NewStat.Identity = stat.Identity;
            NewStat.Entity = stat.Entity;
            NewStat.NodeType = resultStat.NodeType;
        }
    }
}