using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Builders.Behaviors
{
    /// <summary> 
    /// Behavior of Source.SkillConversion.
    /// Applies to Source.SkillConversion.UncappedSubtotal.
    /// Modifies the context to only return paths with skill sources.
    /// </summary>
    public class SkillConversionUncappedSubtotalValue : IValue
    {
        private readonly IStat _skillConversion;
        private readonly IValue _transformedValue;

        public SkillConversionUncappedSubtotalValue(IStat skillConversion, IValue transformedValue)
        {
            _skillConversion = skillConversion;
            _transformedValue = transformedValue;
        }

        public NodeValue? Calculate(IValueCalculationContext context)
        {
            var modifiedContext = new ModifiedValueCalculationContext(context, GetPaths);
            return _transformedValue.Calculate(modifiedContext);
        }

        private IReadOnlyCollection<PathDefinition> GetPaths(IValueCalculationContext context, IStat stat)
        {
            if (!_skillConversion.Equals(stat))
                return context.GetPaths(stat);

            return context.GetPaths(stat)
                .Where(p => p.ModifierSource is ModifierSource.Local.Skill)
                .ToList();
        }
    }
}