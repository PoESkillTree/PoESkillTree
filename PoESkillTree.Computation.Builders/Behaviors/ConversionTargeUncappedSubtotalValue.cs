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

        public ConversionTargeUncappedSubtotalValue(IStat target, IStat source, IValue transformedValue)
        {
            _target = target;
            _source = source;
            _transformedValue = transformedValue;
        }

        public NodeValue? Calculate(IValueCalculationContext context)
        {
            var modifiedContext = new ModifiedContext(_target, _source, context);
            return _transformedValue.Calculate(modifiedContext);
        }

        private class ModifiedContext : IValueCalculationContext
        {
            private readonly IStat _target;
            private readonly IStat _source;
            private readonly IValueCalculationContext _originalContext;

            public ModifiedContext(IStat target, IStat source, IValueCalculationContext originalContext)
            {
                _target = target;
                _source = source;
                _originalContext = originalContext;
            }

            public IEnumerable<PathDefinition> GetPaths(IStat stat)
            {
                if (!_target.Equals(stat))
                    return _originalContext.GetPaths(stat);

                var originalPaths = _originalContext.GetPaths(_target);
                var conversionPaths = _originalContext.GetPaths(_source)
                    .Select(p => new PathDefinition(p.ModifierSource, _source.Concat(p.ConversionStats).ToList()));
                return originalPaths.Concat(conversionPaths).Distinct();
            }

            public NodeValue? GetValue(IStat stat, NodeType nodeType, PathDefinition path) =>
                _originalContext.GetValue(stat, nodeType, path);

            public IEnumerable<NodeValue?> GetValues(Form form, IEnumerable<(IStat stat, PathDefinition path)> paths) =>
                _originalContext.GetValues(form, paths);
        }
    }
}