using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Resolving;

namespace PoESkillTree.Computation.Common.Builders.Values
{
    public class ConditionBuilderStub : IConditionBuilder
    {
        public bool Condition { get; }

        public ConditionBuilderStub(bool condition)
        {
            Condition = condition;
        }

        public IConditionBuilder Resolve(ResolveContext context) => this;

        public IConditionBuilder And(IConditionBuilder condition) =>
            new ConditionBuilderStub(Condition && ((ConditionBuilderStub) condition).Condition);

        public IConditionBuilder Or(IConditionBuilder condition) =>
            new ConditionBuilderStub(Condition || ((ConditionBuilderStub) condition).Condition);

        public IConditionBuilder Not =>
            new ConditionBuilderStub(!Condition);

        public ConditionBuilderResult Build(BuildParameters parameters) => 
            new ConditionBuilderResult(new Constant(Condition));
    }
}