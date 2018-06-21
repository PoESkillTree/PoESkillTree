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
            var conditionsString = "Any(" + string.Join(", ", builtConditions.Select(t => t.Value)) + ")";
            return (ConvertStat, new ConditionalValue(Calculate, conditionsString));

            IStatBuilder ConvertStat(IStatBuilder stat) =>
                builtConditions
                    // TODO if empty, don't create result with converter
                    .Where(t => t.HasStatConverter)
                    .Select(t => t.StatConverter(stat))
                    .Where(s => s != stat)
                    .DefaultIfEmpty(stat)
                    .Aggregate((s1, s2) => s1.CombineWith(s2));

            bool Calculate(IValueCalculationContext context) =>
                builtConditions
                    // TODO if empty, don't create result with value
                    .Where(t => t.HasValue)
                    .Select(t => t.Value.Calculate(context))
                    .Any(v => v.IsTrue());
        }

        public override bool Equals(object obj) =>
            (obj == this) || (obj is OrCompositeConditionBuilder other && Equals(other));

        private bool Equals(OrCompositeConditionBuilder other) =>
            Conditions.SequenceEqual(other.Conditions);

        public override int GetHashCode() =>
            Conditions.SequenceHash();
    }
}