using System;
using PoESkillTree.Common.Utils;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Resolving;

namespace PoESkillTree.Computation.Builders.Conditions
{
    public class ValueConditionBuilder : IConditionBuilder
    {
        private readonly Predicate<IValueCalculationContext> _calculate;

        public ValueConditionBuilder(Predicate<IValueCalculationContext> calculate)
        {
            _calculate = calculate;
        }

        public IConditionBuilder Resolve(ResolveContext context) => this;

        public IConditionBuilder And(IConditionBuilder condition) =>
            new AndCompositeConditionBuilder(this, condition);

        public IConditionBuilder Or(IConditionBuilder condition) =>
            new OrCompositeConditionBuilder(this, condition);

        public IConditionBuilder Not => new ValueConditionBuilder(c => !_calculate(c));

        public (StatConverter statConverter, IValue value) Build() =>
            (Funcs.Identity, new ConditionalValue(_calculate));
    }
}