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
            var modifiedContext = new ModifiedContext(_ailmentDamage, _skillDamage, context);
            return _transformedValue.Calculate(modifiedContext);
        }

        private class ModifiedContext : IValueCalculationContext
        {
            private readonly IStat _ailmentDamage;
            private readonly IStat _skillDamage;
            private readonly IValueCalculationContext _originalContext;

            public ModifiedContext(IStat ailmentDamage, IStat skillDamage, IValueCalculationContext originalContext)
            {
                _ailmentDamage = ailmentDamage;
                _skillDamage = skillDamage;
                _originalContext = originalContext;
            }

            public IEnumerable<PathDefinition> GetPaths(IStat stat)
            {
                var originalPaths = _originalContext.GetPaths(stat);
                if (_ailmentDamage.Equals(stat))
                    return originalPaths.Union(_originalContext.GetPaths(_skillDamage));
                return originalPaths;
            }

            public NodeValue? GetValue(IStat stat, NodeType nodeType, PathDefinition path) =>
                _originalContext.GetValue(stat, nodeType, path);

            public IEnumerable<NodeValue?> GetValues(Form form, IEnumerable<(IStat stat, PathDefinition path)> paths) =>
                _originalContext.GetValues(form, paths);
        }
    }
}