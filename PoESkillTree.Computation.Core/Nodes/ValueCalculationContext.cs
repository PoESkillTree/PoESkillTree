using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Nodes
{
    public class ValueCalculationContext : IValueCalculationContext
    {
        private readonly INodeRepository _nodeRepository;
        private readonly ISet<ICalculationNode> _usedNodes = new HashSet<ICalculationNode>();
        private readonly ISet<IObservableCollection> _usedCollections = new HashSet<IObservableCollection>();

        public ValueCalculationContext(INodeRepository nodeRepository)
        {
            _nodeRepository = nodeRepository;
        }

        public NodeValue? GetValue(IStat stat, NodeType nodeType, PathDefinition path)
        {
            var node = _nodeRepository.GetNode(stat, nodeType, path);
            _usedNodes.Add(node);
            return node.Value;
        }

        public IEnumerable<NodeValue?> GetValues(IStat stat, NodeType nodeType)
        {
            var paths = _nodeRepository.GetPaths(stat);
            _usedCollections.Add(paths);
            // As opposed to form/modifier nodes, stat subgraph nodes are unique. No need for distinctness here.
            return paths.Select(path => GetValue(stat, nodeType, path));
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