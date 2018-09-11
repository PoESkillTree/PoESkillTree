using System.Collections.Generic;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Builders.Behaviors
{
    public class AffectedByModifiersToOtherStatValue : IValue
    {
        private readonly IStat _transformedStat;
        private readonly IStat _otherStat;
        private readonly IStat _conditionStat;
        private readonly Form _affectedForm;
        private readonly IValue _transformedValue;

        public AffectedByModifiersToOtherStatValue(
            IStat transformedStat, IStat otherStat, IStat conditionStat, Form affectedForm, IValue transformedValue)
        {
            (_transformedStat, _otherStat, _conditionStat, _affectedForm, _transformedValue) =
                (transformedStat, otherStat, conditionStat, affectedForm, transformedValue);
        }

        public NodeValue? Calculate(IValueCalculationContext context)
        {
            var modifiedContext = new ModifiedValueCalculationContext(context, getValues: GetValues);
            return _transformedValue.Calculate(modifiedContext);
        }

        private IEnumerable<NodeValue?>
            GetValues(IValueCalculationContext context, Form form, IEnumerable<(IStat stat, PathDefinition path)> paths)
        {
            if (_affectedForm != form || !context.GetValue(_conditionStat).IsTrue())
                return context.GetValues(form, paths);
            return context.GetValues(form, AppendOtherStat(paths));
        }

        private IEnumerable<(IStat stat, PathDefinition path)>
            AppendOtherStat(IEnumerable<(IStat stat, PathDefinition path)> paths)
        {
            foreach (var (stat, path) in paths)
            {
                yield return (stat, path);
                if (stat.Equals(_transformedStat))
                    yield return (_otherStat, path);
            }
        }
    }
}