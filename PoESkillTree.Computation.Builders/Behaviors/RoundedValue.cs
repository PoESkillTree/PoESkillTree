using System;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Builders.Behaviors
{
    public class RoundedValue : IValue
    {
        private readonly IValue _transformedValue;
        private readonly int _decimals;

        public RoundedValue(IValue transformedValue, int decimals)
        {
            _transformedValue = transformedValue;
            _decimals = decimals;
        }

        public NodeValue? Calculate(IValueCalculationContext context)
        {
            var value = _transformedValue.Calculate(context);
            return value.Select(d => Math.Round(d, _decimals));
        }
    }
}