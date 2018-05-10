using System;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;
using PoESkillTree.Computation.Core.Graphs;
using PoESkillTree.Computation.Core.NodeCollections;
using PoESkillTree.Computation.Core.Nodes;

namespace PoESkillTree.Computation.Core
{
    public class StatNodeFactory : IStatNodeFactory
    {
        private readonly INodeFactory _nodeFactory;
        private readonly IStat _stat;

        public StatNodeFactory(INodeFactory nodeFactory, IStat stat)
        {
            _nodeFactory = nodeFactory;
            _stat = stat;
        }

        public IDisposableNodeViewProvider Create(NodeSelector selector)
        {
            switch (selector.NodeType)
            {
                case NodeType.Total:
                    return Create(new TotalValue(_stat));
                case NodeType.Subtotal:
                    return Create(new SubtotalValue(_stat));
                case NodeType.UncappedSubtotal:
                    return Create(new UncappedSubtotalValue(_stat));
                case NodeType.PathTotal:
                    return Create(new UncappedSubtotalValue(_stat));
                case NodeType.Base:
                    return Create(new BaseValue(_stat));
                case NodeType.BaseOverride:
                    return CreateFormAggregatingNode(_stat, Form.BaseOverride, NodeValueAggregators.CalculateOverride);
                case NodeType.BaseSet:
                    return CreateFormAggregatingNode(_stat, Form.BaseSet, NodeValueAggregators.CalculateBaseAdd);
                case NodeType.BaseAdd:
                    return CreateFormAggregatingNode(_stat, Form.BaseAdd, NodeValueAggregators.CalculateBaseAdd);
                case NodeType.Increase:
                    return CreateFormAggregatingNode(_stat, Form.Increase, NodeValueAggregators.CalculateIncrease);
                case NodeType.More:
                    return CreateFormAggregatingNode(_stat, Form.More, NodeValueAggregators.CalculateMore);
                case NodeType.TotalOverride:
                    return CreateFormAggregatingNode(_stat, Form.TotalOverride, NodeValueAggregators.CalculateOverride);
                default:
                    throw new ArgumentOutOfRangeException(nameof(selector), selector.NodeType, null);
            }
        }

        private IDisposableNodeViewProvider CreateFormAggregatingNode(
            IStat stat, Form form, NodeValueAggregator aggregator) =>
            Create(new FormAggregatingValue(stat, form, aggregator));

        private IDisposableNodeViewProvider Create(IValue value) => _nodeFactory.Create(value);

        public ModifierNodeCollection Create(FormNodeSelector selector)
        {
            var defaultView = new NodeCollection<Modifier>();
            var suspendableView = new NodeCollection<Modifier>();
            var viewProvider = SuspendableEventViewProvider.Create(defaultView, suspendableView);
            return new ModifierNodeCollection(viewProvider);
        }
    }
}