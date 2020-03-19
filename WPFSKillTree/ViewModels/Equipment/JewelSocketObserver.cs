using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Engine.Utils;
using PoESkillTree.Engine.Utils.Extensions;
using PoESkillTree.SkillTreeFiles;
using PoESkillTree.Utils;
using PoESkillTree.ViewModels.PassiveTree;

namespace PoESkillTree.ViewModels.Equipment
{
    public sealed class JewelSocketObserver : IDisposable
    {
        private readonly ObservableSet<PassiveNodeViewModel> _skilledNodes;
        private IReadOnlyDictionary<ushort, InventoryItemViewModel>? _treeJewelViewModels;

        public JewelSocketObserver(ObservableSet<PassiveNodeViewModel> skilledNodes)
        {
            _skilledNodes = skilledNodes;
            _skilledNodes.CollectionChanged += SkilledNodesOnCollectionChanged;
        }

        public void Dispose()
        {
            _skilledNodes.CollectionChanged -= SkilledNodesOnCollectionChanged;
        }

        public void ResetTreeJewelViewModels()
        {
            _treeJewelViewModels = null;
        }

        public void SetTreeJewelViewModels(IEnumerable<InventoryItemViewModel> treeJewels)
        {
            _treeJewelViewModels = treeJewels
                .Where(j => j.Socket.HasValue)
                .ToDictionary(j => j.Socket!.Value);
            foreach (var treeJewelViewModel in _treeJewelViewModels.Values)
            {
                treeJewelViewModel.IsEnabled = _skilledNodes.Any(n => n.Id == treeJewelViewModel.Socket);
            }
        }

        private void SkilledNodesOnCollectionChanged(object sender, CollectionChangedEventArgs<PassiveNodeViewModel> args)
        {
            if (_treeJewelViewModels is null)
                return;

            foreach (var node in args.RemovedItems)
            {
                _treeJewelViewModels.ApplyIfPresent(node.Id, j => j.IsEnabled = false);
            }
            foreach (var node in args.AddedItems)
            {
                _treeJewelViewModels.ApplyIfPresent(node.Id, j => j.IsEnabled = true);
            }
        }
    }
}