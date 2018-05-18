using System;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.IntegrationTests.Core
{
    internal class Constant : IValue
    {
        private readonly NodeValue? _value;

        public Constant(double value) =>
            _value = new NodeValue(value);

        public Constant(NodeValue? value) =>
            _value = value;

        public NodeValue? Calculate(IValueCalculationContext valueCalculationContext) =>
            _value;
    }

    internal class PerStatValue : IValue
    {
        private readonly IStat _stat;
        private readonly double _multiplier;
        private readonly double _divisor;

        public PerStatValue(IStat stat, double multiplier, double divisor = 1)
        {
            _stat = stat;
            _multiplier = multiplier;
            _divisor = divisor;
        }

        public NodeValue? Calculate(IValueCalculationContext valueCalculationContext) =>
            _multiplier * (valueCalculationContext.GetValue(_stat) / _divisor).Select(Math.Ceiling);
    }
}