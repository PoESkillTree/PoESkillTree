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
            var modifiedContext = new ModifiedValueCalculationContext(context, getValues: GetValues);
            return _transformedValue.Calculate(modifiedContext);
        }

        private IEnumerable<NodeValue?>
            GetValues(IValueCalculationContext context, Form form, IEnumerable<(IStat stat, PathDefinition path)> paths)
        {
            if (form != Form.Increase && form != Form.More)
                return context.GetValues(form, paths);

            var dealtDamageType = (DamageType) context.GetValue(_dealtDamageType).Single();
            var dealtDamageStat = _ailmentDamageFactory(dealtDamageType);
            return context.GetValues(form, AppendDealtDamage(dealtDamageStat, paths));
        }

        private IEnumerable<(IStat stat, PathDefinition path)>
            AppendDealtDamage(IStat dealtDamageStat, IEnumerable<(IStat stat, PathDefinition path)> paths)
        {
            foreach (var (stat, path) in paths)
            {
                yield return (stat, path);
                if (stat.Equals(_ailmentDamage))
                    yield return (dealtDamageStat, path);
            }
        }
    }
}