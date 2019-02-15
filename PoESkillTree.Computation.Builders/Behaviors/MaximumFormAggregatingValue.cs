using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Builders.Behaviors
{
    public class MaximumFormAggregatingValue : IValue
    {
        private readonly IStat _stat;
        private readonly Form _form;
        private readonly IValue _transformedValue;

        public MaximumFormAggregatingValue(IStat stat, Form form, IValue transformedValue)
            => (_stat, _form, _transformedValue) = (stat, form, transformedValue);

        public NodeValue? Calculate(IValueCalculationContext context)
        {
            var modifiedContext = new ModifiedValueCalculationContext(context, getValues: GetValues);
            return _transformedValue.Calculate(modifiedContext);
        }

        private List<NodeValue?>
            GetValues(IValueCalculationContext context, Form form, IEnumerable<(IStat stat, PathDefinition path)> paths)
        {
            var enumeratedPaths = paths.ToList();
            var originalValues = context.GetValues(form, enumeratedPaths);
            if (form != _form
                || !enumeratedPaths.Any(p => p.stat.Equals(_stat)))
                return originalValues;

            if (!enumeratedPaths.All(p => p.stat.Equals(_stat)))
                throw new InvalidOperationException(
                    "Behavior is undefined when transformed values request values by mixing paths with and without " +
                    "the affected stat.\n" +
                    $"Stat: {_stat}. Form: {_form}. Transformed value: {_transformedValue}");

            var nonNullValues = originalValues.OfType<NodeValue>().ToList();
            var max = nonNullValues.Any() ? nonNullValues.MaxBy(v => v.Single).First() : (NodeValue?) null;
            return new List<NodeValue?> { max };
        }
    }
}