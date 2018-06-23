using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Resolving;
using static PoESkillTree.Computation.Console.Builders.BuilderFactory;

namespace PoESkillTree.Computation.Console.Builders
{
    public class ConditionBuilderStub : BuilderStub, IConditionBuilder
    {
        private readonly Resolver<IConditionBuilder> _resolver;

        public ConditionBuilderStub(string stringRepresentation, Resolver<IConditionBuilder> resolver)
            : base(stringRepresentation)
        {
            _resolver = resolver;
        }

        private IConditionBuilder This => this;

        public IConditionBuilder And(IConditionBuilder condition) =>
            CreateCondition(This, condition, (l, r) => $"{l} and {r}");

        public IConditionBuilder Or(IConditionBuilder condition) =>
            CreateCondition(This, condition, (l, r) => $"{l} or {r}");

        public IConditionBuilder Not =>
            CreateCondition(This, o => $"Not({o})");

        public ConditionBuilderResult Build(BuildParameters parameters) =>
            new ConditionBuilderResult(new ValueStub(this));

        public IConditionBuilder Resolve(ResolveContext context) =>
            _resolver(this, context);
    }
}