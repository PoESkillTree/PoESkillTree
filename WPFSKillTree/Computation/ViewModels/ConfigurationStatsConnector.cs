using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using POESKillTree.Computation.Model;

namespace POESKillTree.Computation.ViewModels
{
    public class ConfigurationStatsConnector
    {
        private readonly Func<ConfigurationStats> _modelProvider;

        private ConfigurationStatsConnector(Func<ConfigurationStats> modelProvider)
        {
            _modelProvider = modelProvider;
        }

        public static void Connect(
            Func<ConfigurationStats> modelProvider, ObservableCollection<ConfigurationStatViewModel> viewModels)
        {
            var connector = new ConfigurationStatsConnector(modelProvider);
            viewModels.CollectionChanged += connector.ViewModelsOnCollectionChanged;
            connector.Added(viewModels.Select(x => x.Node));
        }

        private void ViewModelsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
                throw new NotSupportedException("NotifyCollectionChangedAction.Reset is not supported");

            Added(SelectNodes(e.NewItems));
            Removed(SelectNodes(e.OldItems));
        }

        private static IEnumerable<CalculationNodeViewModel> SelectNodes(IList items)
            => items?.Cast<ConfigurationStatViewModel>().Select(x => x.Node)
               ?? Enumerable.Empty<CalculationNodeViewModel>();

        private void Added(IEnumerable<CalculationNodeViewModel> nodes)
        {
            var model = _modelProvider();
            foreach (var addedNode in nodes)
            {
                if (model.TryGetValue(addedNode.Stat, out var value))
                    addedNode.Value = value;
                addedNode.PropertyChanged += NodeOnPropertyChanged;
            }
        }

        private void Removed(IEnumerable<CalculationNodeViewModel> nodes)
        {
            foreach (var removedNode in nodes)
            {
                removedNode.PropertyChanged -= NodeOnPropertyChanged;
            }
        }

        private void NodeOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(CalculationNodeViewModel.Value))
                return;

            var node = (CalculationNodeViewModel) sender;
            _modelProvider().SetValue(node.Stat, node.Value);
        }
    }
}