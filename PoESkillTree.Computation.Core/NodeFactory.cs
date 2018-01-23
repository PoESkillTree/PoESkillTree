using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    // Creates the ICalculationNodes for the stat subgraph by NodeType
    // It might be a good idea for this to implement INodeRepository
    public class NodeFactory
    {
        private readonly INodeRepository _nodeRepository;

        public NodeFactory(INodeRepository nodeRepository)
        {
            _nodeRepository = nodeRepository;
        }

        public ICalculationNode Create(IStat stat, NodeType nodeType)
        {
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

        private ICalculationNode CreateTotalNode(IStat stat) => 
            new OverwritableNode(GetNode(stat, NodeType.Subtotal), GetNode(stat, NodeType.TotalOverride));

        private ICalculationNode CreateSubtotalNode(IStat stat) =>
            new SubtotalNode(GetNode(stat, NodeType.UncappedSubtotal), GetNode(stat.Minimum), GetNode(stat.Maximum));

        private ICalculationNode CreateUncappedsubtotalNode(IStat stat) =>
            new AggregatingNode(
                GetNodes(stat, NodeType.Base, NodeType.Increase, NodeType.More),
                NodeValueAggregators.CalculateUncappedSubtotal);

        private ICalculationNode CreateBaseNode(IStat stat) =>
            new OverwritableNode(
                new AggregatingNode(
                    GetNodes(stat, NodeType.BaseSet, NodeType.BaseAdd),
                    NodeValueAggregators.CalculateUncappedSubtotal),
                GetNode(stat, NodeType.BaseOverride));

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

        private INodeCollection GetNodes(IStat stat, params NodeType[] nodeTypes)
        {
            var nodes = nodeTypes
                .Select(t => GetNode(stat, t))
                .Select(n => new NodeCollectionItem(n))
                .ToList();
            return new FixedNodeCollection(nodes);
        }

        private ICalculationNode GetNode(IStat stat, NodeType nodeType = NodeType.Total) => 
            _nodeRepository.GetNode(stat, nodeType);

        private INodeCollection GetFormNodes(IStat stat, Form form) =>
            _nodeRepository.GetFormNodes(stat, form);


        private class FixedNodeCollection : INodeCollection
        {
            public FixedNodeCollection(IReadOnlyList<NodeCollectionItem> items)
            {
                Items = items;
            }

            public IReadOnlyList<NodeCollectionItem> Items { get; }
            public event EventHandler ItemsChanged;
        }
    }
}