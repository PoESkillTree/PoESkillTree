using System;
using JetBrains.Annotations;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    public class NodeFactory : INodeFactory
    {
        public INodeRepository NodeRepository { private get; set; }

        public ISuspendableEventViewProvider<ICalculationNode> Create(IValue value) => 
            WrapCoreNode(CreateCoreNode(value));

        public ISuspendableEventViewProvider<ICalculationNode> Create( IStat stat, NodeType nodeType) => 
            WrapCoreNode(CreateCoreNode(stat, nodeType));

        private static ISuspendableEventViewProvider<ICalculationNode> WrapCoreNode(ICalculationNode coreNode)
        {
            var cachingNode = new CachingNode(coreNode);
            var cachingNodeAdapter = new CachingNodeAdapter(cachingNode);
            return SuspendableEventViewProvider.Create<ICalculationNode, ICachingNode>(cachingNodeAdapter, cachingNode);
        }

        private ICalculationNode CreateCoreNode([CanBeNull] IStat stat, NodeType nodeType)
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

        private ICalculationNode CreateCoreNode(IValue value) =>
            new ValueNode(NodeRepository, value);

        private ICalculationNode CreateTotalNode(IStat stat) => 
            CreateCoreNode(new TotalValue(stat));

        private ICalculationNode CreateSubtotalNode(IStat stat) =>
            CreateCoreNode(new SubtotalValue(stat));

        private ICalculationNode CreateUncappedsubtotalNode(IStat stat) =>
            CreateCoreNode(new UncappedSubtotalValue(stat));

        private ICalculationNode CreateBaseNode(IStat stat) =>
            CreateCoreNode(new BaseValue(stat));

        private ICalculationNode CreateBaseOverrideNode(IStat stat) =>
            new AggregatingNode(GetFormNodes(stat, Form.BaseOverride), NodeValueAggregators.CalculateOverride);

        private ICalculationNode CreateBaseSetNode(IStat stat) =>
            new AggregatingNode(GetFormNodes(stat, Form.BaseSet), NodeValueAggregators.CalculateBaseAdd);

        private ICalculationNode CreateBaseAddNode(IStat stat) =>
            new AggregatingNode(GetFormNodes(stat, Form.BaseAdd), NodeValueAggregators.CalculateBaseAdd);

        private ICalculationNode CreateIncreaseNode(IStat stat) =>
            new AggregatingNode(GetFormNodes(stat, Form.Increase), NodeValueAggregators.CalculateIncrease);

        private ICalculationNode CreateMoreNode(IStat stat) =>
            new AggregatingNode(GetFormNodes(stat, Form.More), NodeValueAggregators.CalculateMore);

        private ICalculationNode CreateTotalOverrideNode(IStat stat) =>
            new AggregatingNode(GetFormNodes(stat, Form.TotalOverride), NodeValueAggregators.CalculateOverride);

        private INodeCollection GetFormNodes(IStat stat, Form form) =>
            NodeRepository.GetFormNodeCollection(stat, form);
    }
}