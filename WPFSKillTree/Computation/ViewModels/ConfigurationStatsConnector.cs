using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using MoreLinq;
using PoESkillTree.Utils;
using PoESkillTree.Computation.Model;
using PoESkillTree.Model;
using PoESkillTree.Model.Builds;

namespace PoESkillTree.Computation.ViewModels
{
    public class ConfigurationStatsConnector
    {
        private readonly IPersistentData _persistentData;
        private readonly IEnumerable<ConfigurationNodeViewModel> _nodes;

        private readonly SimpleMonitor _setNodeValueMonitor = new SimpleMonitor();

        public ConfigurationStatsConnector(
            IPersistentData persistentData, IEnumerable<ConfigurationNodeViewModel> nodes)
        {
            _persistentData = persistentData;
            _nodes = nodes;
        }

        public static void Connect(
            IPersistentData persistentData, ObservableCollection<ConfigurationStatViewModel> viewModels,
            IEnumerable<ConfigurationNodeViewModel> additionalNodes)
        {
            var nodes = viewModels.Select(x => x.Node).Concat(additionalNodes);
            var connector = new ConfigurationStatsConnector(persistentData, nodes);
            connector.Initialize(viewModels);
        }

        public void Initialize(INotifyCollectionChanged viewModels)
        {
            _persistentData.PropertyChanging += PersistentDataOnPropertyChanging;
            _persistentData.PropertyChanged += PersistentDataOnPropertyChanged;
            _persistentData.CurrentBuild.PropertyChanged += CurrentBuildOnPropertyChanged;
            viewModels.CollectionChanged += ViewModelsOnCollectionChanged;
            Added(_nodes);
        }

        private ConfigurationStats ConfigurationStats => _persistentData.CurrentBuild.ConfigurationStats;

        private void ViewModelsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
                throw new NotSupportedException("NotifyCollectionChangedAction.Reset is not supported");

            Added(SelectNodes(e.NewItems));
            Removed(SelectNodes(e.OldItems));
        }

        private static IEnumerable<ConfigurationNodeViewModel> SelectNodes(IList items)
            => items?.Cast<ConfigurationStatViewModel>().Select(x => x.Node)
               ?? Enumerable.Empty<ConfigurationNodeViewModel>();

        private void Added(IEnumerable<ConfigurationNodeViewModel> nodes)
        {
            foreach (var addedNode in nodes)
            {
                SetNodeValue(addedNode);
                addedNode.PropertyChanged += NodeOnPropertyChanged;
            }
        }

        private void Removed(IEnumerable<ConfigurationNodeViewModel> nodes)
        {
            foreach (var removedNode in nodes)
            {
                removedNode.PropertyChanged -= NodeOnPropertyChanged;
            }
        }

        private void NodeOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(CalculationNodeViewModel.Value) || _setNodeValueMonitor.IsBusy)
                return;

            var node = (CalculationNodeViewModel) sender;
            ConfigurationStats.SetValue(node.Stat, node.Value);
        }

        private void PersistentDataOnPropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            if (e.PropertyName == nameof(IPersistentData.CurrentBuild))
            {
                _persistentData.CurrentBuild.PropertyChanged -= CurrentBuildOnPropertyChanged;
                _setNodeValueMonitor.Enter();
            }
        }

        private void PersistentDataOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IPersistentData.CurrentBuild))
            {
                _persistentData.CurrentBuild.PropertyChanged += CurrentBuildOnPropertyChanged;
                _nodes.ForEach(SetNodeValue);
                _setNodeValueMonitor.Leave();
            }
        }

        private void CurrentBuildOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PoEBuild.ConfigurationStats))
            {
                _nodes.ForEach(SetNodeValue);
            }
        }

        private void SetNodeValue(ConfigurationNodeViewModel node)
        {
            using (_setNodeValueMonitor.Enter())
            {
                if (ConfigurationStats.TryGetValue(node.Stat, out var value))
                    node.Value = value;
                else
                    node.ResetValue();
            }
        }
    }
}