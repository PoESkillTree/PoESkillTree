using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Computation.Core.Nodes
{
    /// <summary>
    /// Implementation of <see cref="IValueCalculationContext"/> using <see cref="INodeRepository"/>.
    /// Stores the <see cref="ICalculationNode"/>s and <see cref="IObservableCollection"/>s used to allow subscribing
    /// to them.
    /// </summary>
    public class ValueCalculationContext : IValueCalculationContext
    {
        private readonly INodeRepository _nodeRepository;
        private readonly ISet<ICalculationNode> _usedNodes = new HashSet<ICalculationNode>();
        private readonly ISet<IObservableCollection> _usedCollections = new HashSet<IObservableCollection>();

        public ValueCalculationContext(INodeRepository nodeRepository, PathDefinition currentPath)
        {
            _nodeRepository = nodeRepository;
            CurrentPath = currentPath;
        }

        public PathDefinition CurrentPath { get; }

        public NodeValue? GetValue(IStat stat, NodeType nodeType, PathDefinition path)
        {
            var node = _nodeRepository.GetNode(stat, nodeType, path);
            _usedNodes.Add(node);
            return node.Value;
        }

        public IEnumerable<PathDefinition> GetPaths(IStat stat)
        {
            var paths = _nodeRepository.GetPaths(stat);
            _usedCollections.Add(paths);
            return paths;
        }

        public IEnumerable<NodeValue?> GetValues(Form form, IEnumerable<(IStat stat, PathDefinition path)> paths)
        {
            var nodeCollections = 
                paths.Select(p => _nodeRepository.GetFormNodeCollection(p.stat, form, p.path)).ToList();
            _usedCollections.UnionWith(nodeCollections);
            var nodes = nodeCollections.Flatten().Select(t => t.node).ToHashSet();
            _usedNodes.UnionWith(nodes);
            return nodes.Select(n => n.Value);
        }

        public IEnumerable<ICalculationNode> UsedNodes => _usedNodes;
        public IEnumerable<IObservableCollection> UsedCollections => _usedCollections;

        public void Clear()
        {
            _usedNodes.Clear();
            _usedCollections.Clear();
        }
    }
}