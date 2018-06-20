using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Resolving;

namespace PoESkillTree.Computation.Builders.Conditions
{
    public abstract class ConditionBuilderBase : IConditionBuilder
    {
        public abstract IConditionBuilder Resolve(ResolveContext context);

        public IConditionBuilder And(IConditionBuilder condition) =>
            new AndCompositeConditionBuilder(this, condition);

        public IConditionBuilder Or(IConditionBuilder condition) =>
            new OrCompositeConditionBuilder(this, condition);

        public abstract IConditionBuilder Not { get; }

        public abstract (StatConverter statConverter, IValue value) Build(BuildParameters parameters);
    }
}