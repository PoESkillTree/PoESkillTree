using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Builders.Behaviors
{
    public class DamageEffectivenessBaseValue : IValue
    {
        private readonly IStat _transformedStat;
        private readonly IStat _baseSetEffectivenessStat;
        private readonly IStat _baseAddEffectivenessStat;
        private readonly IValue _transformedValue;

        public DamageEffectivenessBaseValue(
            IStat transformedStat, IStat baseSetEffectivenessStat, IStat baseAddEffectivenessStat,
            IValue transformedValue)
        {
            (_transformedStat, _baseSetEffectivenessStat, _baseAddEffectivenessStat, _transformedValue) =
                (transformedStat, baseSetEffectivenessStat, baseAddEffectivenessStat, transformedValue);
        }

        public NodeValue? Calculate(IValueCalculationContext context)
        {
            var modifiedContext = new ModifiedValueCalculationContext(context, getValue: GetValues);
            return _transformedValue.Calculate(modifiedContext);
        }

        private NodeValue? GetValues(
            IValueCalculationContext context, IStat stat, NodeType nodeType, PathDefinition path)
        {
            var originalValue = context.GetValue(stat, nodeType, path);
            if (nodeType != NodeType.BaseAdd && nodeType != NodeType.BaseSet)
                return originalValue;
            if (!stat.Equals(_transformedStat))
                return originalValue;

            var effectivenessStat =
                nodeType == NodeType.BaseSet ? _baseSetEffectivenessStat : _baseAddEffectivenessStat;
            var effectiveness = context.GetValue(effectivenessStat) ?? new NodeValue(1);
            return originalValue * effectiveness;
        }
    }
}