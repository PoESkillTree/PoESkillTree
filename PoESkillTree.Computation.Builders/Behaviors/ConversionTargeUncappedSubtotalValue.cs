using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Builders.Behaviors
{
    /// <summary>
    /// Behavior of Source.ConvertTo(Target) and Source.GainAs(Target).
    /// Applies to Target.UncappedSubtotal.
    /// Modifies the context to append the missing conversion paths from Source when querying Target's paths.
    /// </summary>
    public class ConversionTargeUncappedSubtotalValue : IValue
    {
        private readonly IStat _target;
        private readonly IStat _source;
        private readonly IValue _transformedValue;

        public ConversionTargeUncappedSubtotalValue(IStat source, IStat target, IValue transformedValue)
        {
            _target = target;
            _source = source;
            _transformedValue = transformedValue;
        }

        public NodeValue? Calculate(IValueCalculationContext context)
        {
            var modifiedContext = new ModifiedValueCalculationContext(context, GetPaths);
            return _transformedValue.Calculate(modifiedContext);
        }

        private IEnumerable<PathDefinition> GetPaths(IValueCalculationContext context, IStat stat)
        {
            if (!_target.Equals(stat))
                return context.GetPaths(stat);

            var originalPaths = context.GetPaths(_target);
            var conversionPaths = context.GetPaths(_source)
                .Select(p => new PathDefinition(p.ModifierSource, _source.Concat(p.ConversionStats).ToList()));
            return originalPaths.Concat(conversionPaths).Distinct();
        }
    }
}