using System;
using System.Linq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;
using PoESkillTree.Computation.Core.Graphs;
using PoESkillTree.Computation.Core.NodeCollections;
using PoESkillTree.Computation.Core.Nodes;
using static PoESkillTree.Computation.Core.Nodes.NodeValueAggregators;

namespace PoESkillTree.Computation.Core
{
    /// <summary>
    /// Implementation of <see cref="IStatNodeFactory"/> using the implementations of this project.
    /// </summary>
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
            var (nodeType, path) = selector;
            switch (nodeType)
            {
                case NodeType.Total:
                    return Create(new TotalValue(_stat), path);
                case NodeType.Subtotal:
                    return Create(new SubtotalValue(_stat), path);
                case NodeType.UncappedSubtotal:
                    return Create(new UncappedSubtotalValue(_stat), path);
                case NodeType.PathTotal:
                    return Create(new PathTotalValue(_stat, path), path);
                case NodeType.Base when path.ConversionStats.Any():
                    return Create(new ConvertedBaseValue(path), path);
                case NodeType.Base:
                    return Create(new BaseValue(_stat, path), path);
                case NodeType.BaseSet:
                    return Create(new FormAggregatingValue(_stat, Form.BaseSet, path, CalculateBaseSet), path);
                case NodeType.BaseAdd:
                    return Create(new FormAggregatingValue(_stat, Form.BaseAdd, path, CalculateBaseAdd), path);
                case NodeType.Increase:
                    return Create(new MultiPathFormAggregatingValue(_stat, Form.Increase, path, CalculateIncrease),
                        path);
                case NodeType.More:
                    return Create(new MultiPathFormAggregatingValue(_stat, Form.More, path, CalculateMore), path);
                case NodeType.TotalOverride:
                    return Create(new FormAggregatingValue(_stat, Form.TotalOverride, path, CalculateTotalOverride),
                        path);
                default:
                    throw new ArgumentOutOfRangeException(nameof(selector), nodeType, null);
            }
        }

        private IDisposableNodeViewProvider Create(IValue value, PathDefinition path)
            => _nodeFactory.Create(value, path);

        public ModifierNodeCollection Create(FormNodeSelector selector)
        {
            var defaultView = new NodeCollection<Modifier>();
            var suspendableView = new NodeCollection<Modifier>();
            var viewProvider = SuspendableEventViewProvider.Create(defaultView, suspendableView);
            return new ModifierNodeCollection(viewProvider);
        }
    }
}