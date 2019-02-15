using System.Collections.Generic;
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

        public ValueCalculationContext(INodeRepository nodeRepository, PathDefinition currentPath)
        {
            _nodeRepository = nodeRepository;
            CurrentPath = currentPath;
        }

        public PathDefinition CurrentPath { get; }

        public NodeValue? GetValue(IStat stat, NodeType nodeType, PathDefinition path)
        {
            var node = _nodeRepository.GetNode(stat, nodeType, path);
            UsedNodes.Add(node);
            return node.Value;
        }

        public IReadOnlyCollection<PathDefinition> GetPaths(IStat stat)
        {
            var paths = _nodeRepository.GetPaths(stat);
            UsedCollections.Add(paths);
            return paths;
        }

        public List<NodeValue?> GetValues(Form form, IEnumerable<(IStat stat, PathDefinition path)> paths)
        {
            var nodeList = new List<ICalculationNode>();
            foreach (var (stat, path) in paths)
            {
                var nodeCollection = _nodeRepository.GetFormNodeCollection(stat, form, path);
                UsedCollections.Add(nodeCollection);
                foreach (var (node, _) in nodeCollection)
                {
                    UsedNodes.Add(node);
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

        public HashSet<ICalculationNode> UsedNodes { get; } = new HashSet<ICalculationNode>();

        public HashSet<IObservableCollection> UsedCollections { get; } = new HashSet<IObservableCollection>();

        public void Clear()
        {
            UsedNodes.Clear();
            UsedCollections.Clear();
        }
    }
}