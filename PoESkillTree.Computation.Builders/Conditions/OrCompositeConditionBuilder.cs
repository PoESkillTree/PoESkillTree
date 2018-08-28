using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Computation.Builders.Conditions
{
    public class OrCompositeConditionBuilder : IConditionBuilder
    {
        public OrCompositeConditionBuilder(params IConditionBuilder[] conditions)
        {
            Conditions = conditions;
        }

        public OrCompositeConditionBuilder(IReadOnlyList<IConditionBuilder> conditions)
        {
            Conditions = conditions;
        }

        public IReadOnlyList<IConditionBuilder> Conditions { get; }

        public IConditionBuilder Resolve(ResolveContext context) =>
            new OrCompositeConditionBuilder(Conditions.Select(c => c.Resolve(context)).ToList());

        public IConditionBuilder And(IConditionBuilder condition) =>
            new AndCompositeConditionBuilder(this, condition);

        public IConditionBuilder Or(IConditionBuilder condition) =>
            new OrCompositeConditionBuilder(Conditions.Append(condition).ToList());

        public IConditionBuilder Not =>
            new AndCompositeConditionBuilder(Conditions.Select(c => c.Not).ToList());

        public ConditionBuilderResult Build(BuildParameters parameters)
        {
            var builtConditions = Conditions.Select(c => c.Build(parameters)).ToList();
            var statConverters = builtConditions.Where(t => t.HasStatConverter).Select(t => t.StatConverter).ToList();
            var values = builtConditions.Where(t => t.HasValue).Select(t => t.Value).ToList();

            var statConverter = statConverters.Any() ? (StatConverter) ConvertStat : null;
            var conditionsString = "Any(" + string.Join(", ", values) + ")";
            var value = values.Any() ? new ConditionalValue(Calculate, conditionsString) : null;
            return new ConditionBuilderResult(statConverter, value);

            IStatBuilder ConvertStat(IStatBuilder stat) =>
                statConverters
                    .Select(c => c(stat))
                    .Where(s => s != stat)
                    .DefaultIfEmpty(stat)
                    .Aggregate((s1, s2) => s1.CombineWith(s2));

            bool Calculate(IValueCalculationContext context) =>
                values.Any(v => v.Calculate(context).IsTrue());
        }

        public override bool Equals(object obj) =>
            (obj == this) || (obj is OrCompositeConditionBuilder other && Equals(other));

        private bool Equals(OrCompositeConditionBuilder other) =>
            Conditions.SequenceEqual(other.Conditions);

        public override int GetHashCode() =>
            Conditions.SequenceHash();
    }
}