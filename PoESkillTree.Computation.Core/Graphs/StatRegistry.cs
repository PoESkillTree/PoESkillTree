using System.Collections.Generic;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;
using PoESkillTree.Computation.Core.NodeCollections;
using PoESkillTree.Computation.Core.Nodes;

namespace PoESkillTree.Computation.Core.Graphs
{
    /// <summary>
    /// Registry of explicitly registered stats used for <see cref="ICalculator.ExplicitlyRegisteredStats"/>.
    /// <para>
    /// Node exposed here are wrapped using <see cref="WrappingNode"/>.
    /// </para>
    /// <para>
    /// Implements <see cref="IDeterminesNodeRemoval"/> by counting subscribers. Nodes can be removed when they
    /// are not subscribed to, except for nodes registered here, those can also be removed when they have one
    /// subscriber (the <see cref="WrappingNode"/>.
    /// </para>
    /// </summary>
    public class StatRegistry : IDeterminesNodeRemoval
    {
        private readonly NodeCollection<IStat> _nodeCollection;

        private readonly Dictionary<IStat, WrappingNode> _registeredWrappedNodes =
            new Dictionary<IStat, WrappingNode>();

        private readonly Dictionary<IStat, ICalculationNode> _registeredNodes =
            new Dictionary<IStat, ICalculationNode>();

        public StatRegistry(NodeCollection<IStat> nodeCollection)
        {
            _nodeCollection = nodeCollection;
        }

        public INodeRepository NodeRepository { private get; set; }

        public void Add(IStat stat)
        {
            if (stat.ExplicitRegistrationType is null)
                return;
            var node = NodeRepository.GetNode(stat);
            _registeredNodes[stat] = node;
            var wrappedNode = new WrappingNode(node);
            _registeredWrappedNodes[stat] = wrappedNode;
            _nodeCollection.Add(wrappedNode, stat);
        }

        public void Remove(IStat stat)
        {
            if (!_registeredWrappedNodes.TryGetValue(stat, out var wrappedNode))
                return;
            wrappedNode.Dispose();
            _registeredNodes.Remove(stat);
            _registeredWrappedNodes.Remove(stat);
            _nodeCollection.Remove(wrappedNode, stat);
        }

        public bool CanBeRemoved(IBufferingEventViewProvider<ICalculationNode> node)
        {
            if (_registeredNodes.ContainsValue(node.BufferingView))
            {
                return node.SubscriberCount <= 1;
            }
            return node.SubscriberCount == 0;
        }

        public bool CanBeRemoved(ICountsSubsribers node) => node.SubscriberCount == 0;
    }
}