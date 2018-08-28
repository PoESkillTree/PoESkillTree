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
    public class AndCompositeConditionBuilder : IConditionBuilder
    {
        public AndCompositeConditionBuilder(params IConditionBuilder[] conditions)
        {
            Conditions = conditions;
        }

        public AndCompositeConditionBuilder(IReadOnlyList<IConditionBuilder> conditions)
        {
            Conditions = conditions;
        }

        public IReadOnlyList<IConditionBuilder> Conditions { get; }

        public IConditionBuilder Resolve(ResolveContext context) =>
            new AndCompositeConditionBuilder(Conditions.Select(c => c.Resolve(context)).ToList());

        public IConditionBuilder And(IConditionBuilder condition) =>
            new AndCompositeConditionBuilder(Conditions.Append(condition).ToList());

        public IConditionBuilder Or(IConditionBuilder condition) =>
            new OrCompositeConditionBuilder(this, condition);

        public IConditionBuilder Not =>
            new OrCompositeConditionBuilder(Conditions.Select(c => c.Not).ToList());

        public ConditionBuilderResult Build(BuildParameters parameters)
        {
            var builtConditions = Conditions.Select(c => c.Build(parameters)).ToList();
            var statConverters = builtConditions.Where(t => t.HasStatConverter).Select(t => t.StatConverter).ToList();
            var values = builtConditions.Where(t => t.HasValue).Select(t => t.Value).ToList();

            var statConverter = statConverters.Any() ? (StatConverter) ConvertStat : null;
            var conditionsString = "All(" + string.Join(", ", values) + ")";
            var value = values.Any() ? new ConditionalValue(Calculate, conditionsString) : null;
            return new ConditionBuilderResult(statConverter, value);

            IStatBuilder ConvertStat(IStatBuilder stat) =>
                statConverters.Aggregate(stat, (s, c) => c(s));

            bool Calculate(IValueCalculationContext context) =>
                values.All(v => v.Calculate(context).IsTrue());
        }

        public override bool Equals(object obj) =>
            (obj == this) || (obj is AndCompositeConditionBuilder other && Equals(other));

        private bool Equals(AndCompositeConditionBuilder other) =>
            Conditions.SequenceEqual(other.Conditions);

        public override int GetHashCode() =>
            Conditions.SequenceHash();
    }
}