using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Builders.Behaviors
{
    public class AilmentDamageUncappedSubtotalValue : IValue
    {
        private readonly IValue _transformedValue;

        public AilmentDamageUncappedSubtotalValue(IValue transformedValue)
        {
            _transformedValue = transformedValue;
        }

        public NodeValue? Calculate(IValueCalculationContext context)
        {
            return _transformedValue.Calculate(context);
        }
    }
}