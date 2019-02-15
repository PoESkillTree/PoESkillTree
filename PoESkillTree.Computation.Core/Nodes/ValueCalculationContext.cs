using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;

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
        private readonly HashSet<ICalculationNode> _usedNodes = new HashSet<ICalculationNode>();
        private readonly HashSet<IObservableCollection> _usedCollections = new HashSet<IObservableCollection>();

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

        public IReadOnlyCollection<PathDefinition> GetPaths(IStat stat)
        {
            var paths = _nodeRepository.GetPaths(stat);
            _usedCollections.Add(paths);
            return paths;
        }

        public List<NodeValue?> GetValues(Form form, IEnumerable<(IStat stat, PathDefinition path)> paths)
        {
            var nodeList = new List<ICalculationNode>();
            foreach (var (stat, path) in paths)
            {
                var nodeCollection = _nodeRepository.GetFormNodeCollection(stat, form, path);
                _usedCollections.Add(nodeCollection);
                foreach (var (node, _) in nodeCollection)
                {
                    _usedNodes.Add(node);
                    nodeList.Add(node);
                }
            }
            
            var nodes = new HashSet<ICalculationNode>(nodeList);
            var values = new List<NodeValue?>(nodes.Count);
            foreach (var node in nodes)
            {
                values.Add(node.Value);
            }
            return values;
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