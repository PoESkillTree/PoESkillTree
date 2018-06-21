using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Skills;
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


    public class ConditionBuildersStub : IConditionBuilders
    {
        public IConditionBuilder With(IKeywordBuilder keyword) =>
            CreateCondition(keyword, o => $"With {o}");

        public IConditionBuilder With(ISkillBuilder skill) =>
            CreateCondition(skill, o => $"With {o}");

        public IConditionBuilder AttackWith(AttackDamageHand hand) =>
            CreateCondition($"Attack with {hand}");

        public IConditionBuilder For(IEntityBuilder entity) =>
            CreateCondition(entity, o => "For " + o);

        public IConditionBuilder BaseValueComesFrom(ItemSlot slot) =>
            CreateCondition($"If base value comes from {slot}");

        public IConditionBuilder Unique(string name) =>
            CreateCondition(name);

        public IConditionBuilder True =>
            CreateCondition("unconditional");
    }
}