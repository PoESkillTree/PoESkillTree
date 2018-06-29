using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Builders.Behaviors
{
    public class AilmentDamageBaseValue : IValue
    {
        private readonly IValue _transformedValue;

        public AilmentDamageBaseValue(IValue transformedValue)
        {
            _transformedValue = transformedValue;
        }

        public NodeValue? Calculate(IValueCalculationContext context)
        {
            return _transformedValue.Calculate(context);
        }
    }
}