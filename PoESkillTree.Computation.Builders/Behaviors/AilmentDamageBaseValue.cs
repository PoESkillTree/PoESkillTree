using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Builders.Behaviors
{
    public class AilmentDamageBaseValue : IValue
    {
        private readonly IStat _skillDamage;
        private readonly IValue _transformedValue;

        public AilmentDamageBaseValue(IStat skillDamage, IValue transformedValue)
        {
            _skillDamage = skillDamage;
            _transformedValue = transformedValue;
        }

        public NodeValue? Calculate(IValueCalculationContext context)
        {
            var original = _transformedValue.Calculate(context);
            var skillBaseDamage = context.GetValue(_skillDamage, NodeType.Base, context.CurrentPath);
            return new[] { original, skillBaseDamage }.Sum();
        }
    }
}