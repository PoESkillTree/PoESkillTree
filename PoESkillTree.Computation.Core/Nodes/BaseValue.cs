using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Nodes
{
    public class BaseValue : IValue
    {
        private readonly IStat _stat;

        public BaseValue(IStat stat)
        {
            _stat = stat;
        }

        public NodeValue? Calculate(IValueCalculationContext context)
        {
            if (context.GetValue(_stat, NodeType.BaseOverride) is NodeValue baseOverride)
            {
                return baseOverride;
            }

            if (context.GetValue(_stat, NodeType.BaseSet) is NodeValue baseSet)
            {
                var baseAdd = context.GetValue(_stat, NodeType.BaseAdd) ?? new NodeValue(0);
                return baseSet + baseAdd;
            }

            return context.GetValue(_stat, NodeType.BaseAdd);
        }
    }
}