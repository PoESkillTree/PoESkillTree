using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Builders.Behaviors
{
    /// <summary> 
    /// Behavior of Source.ConvertTo(Target).
    /// Applies to Source.ConvertTo(Target).UncappedSubtotal
    /// Modifies the context to apply multipliers to Source.ConvertTo(Target).PathTotal nodes (see code for details)
    /// </summary>
    public class ConvertToUncappedSubtotalValue : IValue
    {
        private readonly IStat _convertTo;
        private readonly IStat _conversion;
        private readonly IStat _skillConversion;
        private readonly IValue _transformedValue;

        public ConvertToUncappedSubtotalValue(
            IStat convertTo, IStat conversion, IStat skillConversion, IValue transformedValue)
        {
            _convertTo = convertTo;
            _conversion = conversion;
            _skillConversion = skillConversion;
            _transformedValue = transformedValue;
        }

        public NodeValue? Calculate(IValueCalculationContext context)
        {
            var modifiedContext = new ModifiedValueCalculationContext(context, getValue: GetValue);
            return _transformedValue.Calculate(modifiedContext);
        }

        private NodeValue?
            GetValue(IValueCalculationContext context, IStat stat, NodeType nodeType, PathDefinition path)
        {
            var value = context.GetValue(stat, nodeType, path);
            if (value is null || !_convertTo.Equals(stat) || nodeType != NodeType.PathTotal)
                return value;

            var sourceConversion = context.GetValue(_conversion) ?? new NodeValue(0);
            if (sourceConversion <= 100)
            {
                // Conversions don't exceed 100%, No scaling required
                return value;
            }

            var isSkillPath = path.ModifierSource is ModifierSource.Local.Skill;
            var sourceSkillConversion = context.GetValue(_skillConversion) ?? new NodeValue(0);
            if (sourceSkillConversion >= 100)
            {
                // Conversions from skills are or exceed 100%
                // Non-skill conversions don't apply
                if (!isSkillPath)
                    return new NodeValue(0);
                // Skill conversions are scaled to sum to 100%
                return value / sourceSkillConversion * 100;
            }

            // Conversions exceed 100%
            // Skill conversions don't scale (they themselves don't exceed 100%)
            if (isSkillPath)
                return value;
            // Non-skill conversions are scaled to sum to 100% - skill conversions
            return value * (100 - sourceSkillConversion) / (sourceConversion - sourceSkillConversion);
        }
    }
}