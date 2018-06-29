using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Builders.Behaviors
{
    public class AilmentDamageIncreaseMoreValue : IValue
    {
        private readonly IValue _transformedValue;

        public AilmentDamageIncreaseMoreValue(IValue transformedValue)
        {
            _transformedValue = transformedValue;
        }

        public NodeValue? Calculate(IValueCalculationContext context)
        {
            return _transformedValue.Calculate(context);
        }
    }
}