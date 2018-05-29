using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Nodes
{
    /// <summary>
    /// <see cref="IValue"/> for <see cref="NodeType.Base"/> on non-conversion paths.
    /// </summary>
    public class BaseValue : IValue
    {
        private readonly IStat _stat;
        private readonly PathDefinition _path;

        public BaseValue(IStat stat, PathDefinition path)
        {
            _stat = stat;
            _path = path;
        }

        public NodeValue? Calculate(IValueCalculationContext context)
        {
            if (context.GetValue(_stat, NodeType.BaseOverride, _path) is NodeValue baseOverride)
            {
                return baseOverride;
            }

            if (context.GetValue(_stat, NodeType.BaseSet, _path) is NodeValue baseSet)
            {
                var baseAdd = context.GetValue(_stat, NodeType.BaseAdd, _path) ?? new NodeValue(0);
                return baseSet + baseAdd;
            }

            return context.GetValue(_stat, NodeType.BaseAdd, _path);
        }
    }
}