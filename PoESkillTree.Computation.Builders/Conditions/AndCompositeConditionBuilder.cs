using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;

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

        public (StatConverter statConverter, IValue value) Build()
        {
            var builtConditions = Conditions.Select(c => c.Build()).ToList();
            var conditionsString = "{" + string.Join(", ", builtConditions.Select(t => t.value)) + "}";
            return (ConvertStat, new ConditionalValue(Calculate, conditionsString + ".All()"));

            IStatBuilder ConvertStat(IStatBuilder stat) =>
                builtConditions.Select(t => t.statConverter).Aggregate(stat, (s, c) => c(s));

            bool Calculate(IValueCalculationContext context) =>
                builtConditions
                    .Select(t => t.value.Calculate(context))
                    .All(v => v.IsTrue());
        }

        public override bool Equals(object obj) =>
            (obj == this) || (obj is AndCompositeConditionBuilder other && Equals(other));

        private bool Equals(AndCompositeConditionBuilder other) =>
            Conditions.SequenceEqual(other.Conditions);

        public override int GetHashCode() =>
            Conditions.SequenceHash();
    }
}