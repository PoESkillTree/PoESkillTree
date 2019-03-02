using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Builders.Behaviors
{
    public class AilmentDamageUncappedSubtotalValue : IValue
    {
        private readonly IStat _ailmentDamage;
        private readonly IStat _skillDamage;
        private readonly IValue _transformedValue;

        public AilmentDamageUncappedSubtotalValue(IStat ailmentDamage, IStat skillDamage, IValue transformedValue)
        {
            _ailmentDamage = ailmentDamage;
            _skillDamage = skillDamage;
            _transformedValue = transformedValue;
        }

        public NodeValue? Calculate(IValueCalculationContext context)
        {
            var modifiedContext = new ModifiedValueCalculationContext(context, getPaths: GetPaths);
            return _transformedValue.Calculate(modifiedContext);
        }

        private IReadOnlyCollection<PathDefinition> GetPaths(IValueCalculationContext context, IStat stat)
        {
            var originalPaths = context.GetPaths(stat);
            if (!_ailmentDamage.Equals(stat))
                return originalPaths;

            var skillPaths = context.GetPaths(_skillDamage)
                .Select(p => new PathDefinition(p.ModifierSource));
            var paths = new HashSet<PathDefinition>(originalPaths);
            paths.UnionWith(skillPaths);
            return paths;
        }
    }
}