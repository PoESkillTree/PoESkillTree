using System;
using JetBrains.Annotations;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;

namespace PoESkillTree.Computation.Core.Nodes
{
    public class NodeFactory : INodeFactory
    {
        public INodeRepository NodeRepository { private get; set; }

        public ISuspendableEventViewProvider<IDisposableNode> Create(IValue value) => 
            WrapCoreNode(CreateCoreNode(value));

        public ISuspendableEventViewProvider<IDisposableNode> Create( IStat stat, NodeType nodeType) => 
            WrapCoreNode(CreateCoreNode(stat, nodeType));

        private static ISuspendableEventViewProvider<IDisposableNode> WrapCoreNode(IDisposableNode coreNode)
        {
            var cachingNode = new CachingNode(coreNode);
            var cachingNodeAdapter = new CachingNodeAdapter(cachingNode);
            return SuspendableEventViewProvider.Create<SubscriberCountingNode, CachingNode>(
                cachingNodeAdapter, cachingNode);
        }

        private IDisposableNode CreateCoreNode([CanBeNull] IStat stat, NodeType nodeType)
        {
            if (stat is null)
                return new NullNode();
            switch (nodeType)
            {
                case NodeType.Total:
                    return CreateTotalNode(stat);
                case NodeType.Subtotal:
                    return CreateSubtotalNode(stat);
                case NodeType.UncappedSubtotal:
                    return CreateUncappedsubtotalNode(stat);
                case NodeType.Base:
                    return CreateBaseNode(stat);
                case NodeType.BaseOverride:
                    return CreateBaseOverrideNode(stat);
                case NodeType.BaseSet:
                    return CreateBaseSetNode(stat);
                case NodeType.BaseAdd:
                    return CreateBaseAddNode(stat);
                case NodeType.Increase:
                    return CreateIncreaseNode(stat);
                case NodeType.More:
                    return CreateMoreNode(stat);
                case NodeType.TotalOverride:
                    return CreateTotalOverrideNode(stat);
                default:
                    throw new ArgumentOutOfRangeException(nameof(nodeType), nodeType, null);
            }
        }

        private IDisposableNode CreateCoreNode(IValue value) =>
            new ValueNode(NodeRepository, value);

        private IDisposableNode CreateTotalNode(IStat stat) => 
            CreateCoreNode(new TotalValue(stat));

        private IDisposableNode CreateSubtotalNode(IStat stat) =>
            CreateCoreNode(new SubtotalValue(stat));

        private IDisposableNode CreateUncappedsubtotalNode(IStat stat) =>
            CreateCoreNode(new UncappedSubtotalValue(stat));

        private IDisposableNode CreateBaseNode(IStat stat) =>
            CreateCoreNode(new BaseValue(stat));

        private IDisposableNode CreateBaseOverrideNode(IStat stat) =>
            new AggregatingNode(GetFormNodes(stat, Form.BaseOverride), NodeValueAggregators.CalculateOverride);

        private IDisposableNode CreateBaseSetNode(IStat stat) =>
            new AggregatingNode(GetFormNodes(stat, Form.BaseSet), NodeValueAggregators.CalculateBaseAdd);

        private IDisposableNode CreateBaseAddNode(IStat stat) =>
            new AggregatingNode(GetFormNodes(stat, Form.BaseAdd), NodeValueAggregators.CalculateBaseAdd);

        private IDisposableNode CreateIncreaseNode(IStat stat) =>
            new AggregatingNode(GetFormNodes(stat, Form.Increase), NodeValueAggregators.CalculateIncrease);

        private IDisposableNode CreateMoreNode(IStat stat) =>
            new AggregatingNode(GetFormNodes(stat, Form.More), NodeValueAggregators.CalculateMore);

        private IDisposableNode CreateTotalOverrideNode(IStat stat) =>
            new AggregatingNode(GetFormNodes(stat, Form.TotalOverride), NodeValueAggregators.CalculateOverride);

        private INodeCollection GetFormNodes(IStat stat, Form form) =>
            NodeRepository.GetFormNodeCollection(stat, form);
    }
}