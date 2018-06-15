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
            var modifiedContext = new ModifiedContext(_skillConversion, context);
            return _transformedValue.Calculate(modifiedContext);
        }

        private class ModifiedContext : IValueCalculationContext
        {
            private readonly IStat _skillConversion;
            private readonly IValueCalculationContext _originalContext;

            public ModifiedContext(IStat skillConversion, IValueCalculationContext originalContext)
            {
                _skillConversion = skillConversion;
                _originalContext = originalContext;
            }

            public IEnumerable<PathDefinition> GetPaths(IStat stat)
            {
                if (!_skillConversion.Equals(stat))
                    return _originalContext.GetPaths(stat);

                return _originalContext.GetPaths(stat)
                    .Where(p => p.ModifierSource is ModifierSource.Local.Skill);
            }

            public NodeValue? GetValue(IStat stat, NodeType nodeType, PathDefinition path) =>
                _originalContext.GetValue(stat, nodeType, path);

            public IEnumerable<NodeValue?> GetValues(Form form, IEnumerable<(IStat stat, PathDefinition path)> paths) =>
                _originalContext.GetValues(form, paths);
        }
    }
}