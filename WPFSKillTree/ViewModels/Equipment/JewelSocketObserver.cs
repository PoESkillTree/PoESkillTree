using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Engine.Utils;
using PoESkillTree.Engine.Utils.Extensions;
using PoESkillTree.SkillTreeFiles;
using PoESkillTree.Utils;

namespace PoESkillTree.ViewModels.Equipment
{
    public class JewelSocketObserver : IDisposable
    {
        private readonly ObservableSet<SkillNode> _skilledNodes;
        private IReadOnlyDictionary<ushort, InventoryItemViewModel> _treeJewelViewModels;

        public JewelSocketObserver(ObservableSet<SkillNode> skilledNodes)
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
                .ToDictionary(j => j.Socket.Value);
            foreach (var treeJewelViewModel in _treeJewelViewModels.Values)
            {
                treeJewelViewModel.IsEnabled = _skilledNodes.Any(n => n.Id == treeJewelViewModel.Socket);
            }
        }

        private void SkilledNodesOnCollectionChanged(object sender, CollectionChangedEventArgs<SkillNode> args)
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