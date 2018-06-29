using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Damage;

namespace PoESkillTree.Computation.Builders.Behaviors
{
    public class AilmentDamageIncreaseMoreValue : IValue
    {
        private readonly IStat _ailmentDamage;
        private readonly IStat _dealtDamageType;
        private readonly Func<DamageType, IStat> _ailmentDamageFactory;
        private readonly IValue _transformedValue;

        public AilmentDamageIncreaseMoreValue(IStat ailmentDamage, IStat dealtDamageType,
            Func<DamageType, IStat> ailmentDamageFactory, IValue transformedValue)
        {
            _dealtDamageType = dealtDamageType;
            _transformedValue = transformedValue;
            _ailmentDamageFactory = ailmentDamageFactory;
            _ailmentDamage = ailmentDamage;
        }

        public NodeValue? Calculate(IValueCalculationContext context)
        {
            var dealtDamageType = (DamageType) context.GetValue(_dealtDamageType).Single();
            var dealtDamageStat = _ailmentDamageFactory(dealtDamageType);
            var modifiedContext = new ModifiedContext(_ailmentDamage, dealtDamageStat, context);
            return _transformedValue.Calculate(modifiedContext);
        }

        private class ModifiedContext : IValueCalculationContext
        {
            private readonly IStat _ailmentDamage;
            private readonly IStat _dealtDamage;
            private readonly IValueCalculationContext _originalContext;

            public ModifiedContext(IStat ailmentDamage, IStat dealtDamageType, IValueCalculationContext originalContext)
            {
                _originalContext = originalContext;
                _ailmentDamage = ailmentDamage;
                _dealtDamage = dealtDamageType;
            }

            public IEnumerable<PathDefinition> GetPaths(IStat stat) =>
                _originalContext.GetPaths(stat);

            public NodeValue? GetValue(IStat stat, NodeType nodeType, PathDefinition path) =>
                _originalContext.GetValue(stat, nodeType, path);

            public IEnumerable<NodeValue?> GetValues(Form form, IEnumerable<(IStat stat, PathDefinition path)> paths)
            {
                if (form == Form.Increase || form == Form.More)
                    return _originalContext.GetValues(form, AppendDealtDamage(paths));
                return _originalContext.GetValues(form, paths);
            }

            private IEnumerable<(IStat stat, PathDefinition path)>
                AppendDealtDamage(IEnumerable<(IStat stat, PathDefinition path)> paths)
            {
                foreach (var (stat, path) in paths)
                {
                    yield return (stat, path);
                    if (stat.Equals(_ailmentDamage))
                        yield return (_dealtDamage, path);
                }
            }
        }
    }
}