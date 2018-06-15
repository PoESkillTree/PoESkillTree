using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Builders.Behaviors
{
    /// <summary>
    /// Behavior of Source.ConvertTo(*).
    /// Applies to Source.PathTotal (all paths)
    /// Returns value * (1 - Source.Conversion).Clip(0, 1)
    /// </summary>
    public class ConversionSourcePathTotalValue : IValue
    {
        private readonly IStat _conversion;
        private readonly IValue _transformedValue;

        public ConversionSourcePathTotalValue(IStat conversion, IValue transformedValue)
        {
            _conversion = conversion;
            _transformedValue = transformedValue;
        }

        public NodeValue? Calculate(IValueCalculationContext context)
        {
            var value = _transformedValue.Calculate(context);
            if (value is null)
                return null;

            var conversion = context.GetValue(_conversion) ?? new NodeValue(0);
            return value * (1 - conversion / 100).Clip(0, 1);
        }
    }
}