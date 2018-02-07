using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;
using PoESkillTree.Computation.Core.Graphs;
using PoESkillTree.Computation.Core.Nodes;

namespace PoESkillTree.Computation.Core
{
    public class NodeFactory : INodeFactory
    {
        public INodeRepository NodeRepository { private get; set; }

        public ISuspendableEventViewProvider<IDisposableNode> Create(IValue value)
        {
            var coreNode = new ValueNode(new ValueCalculationContext(NodeRepository), value);
            var cachingNode = new CachingNode(coreNode, new CycleGuard());
            var cachingNodeAdapter = new CachingNodeAdapter(cachingNode);
            return SuspendableEventViewProvider.Create<SubscriberCountingNode, CachingNode>(
                cachingNodeAdapter, cachingNode);
        }
    }
}