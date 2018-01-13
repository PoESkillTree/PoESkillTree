using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;
using static PoESkillTree.Computation.Console.Builders.BuilderFactory;

namespace PoESkillTree.Computation.Console.Builders
{
    public class DamageStatBuilderStub : StatBuilderStub, IDamageStatBuilder
    {
        public DamageStatBuilderStub(string stringRepresentation, Resolver<IStatBuilder> resolver)
            : base(stringRepresentation, resolver)
        {
        }

        public IStatBuilder Taken => CreateStat(This, o => $"{o} taken");

        public IDamageTakenConversionBuilder TakenFrom(IPoolStatBuilder pool) =>
            Create<IDamageTakenConversionBuilder, IStatBuilder, IStatBuilder>(
                (s, r) => new DamageTakenConversionBuilder(s, r),
                This, pool,
                (o1, o2) => $"{o1} taken from {o2}");

        public IConditionBuilder With() =>
            CreateCondition(This, o => $"With {o}");

        public IConditionBuilder With(IDamageSourceBuilder source) =>
            CreateCondition(This, source, (o1, o2) => $"With {o2} {o1}");

        public IConditionBuilder With(Tags tags) =>
            CreateCondition(This, o => $"With {tags} {o}");

        public IConditionBuilder With(IAilmentBuilder ailment) =>
            CreateCondition(This, (IEffectBuilder) ailment, (o1, o2) => $"With {o2} {o1}");

        public IConditionBuilder With(ItemSlot slot) =>
            CreateCondition(This, o => $"With {slot} {o}");

        public override IStatBuilder WithCondition(IConditionBuilder condition) =>
            CreateDamageStat(This, condition, (s, c) => $"{s} ({c})");


        private class DamageTakenConversionBuilder : BuilderStub, IDamageTakenConversionBuilder
        {
            private readonly Resolver<IDamageTakenConversionBuilder> _resolver;

            public DamageTakenConversionBuilder(
                string stringRepresentation, Resolver<IDamageTakenConversionBuilder> resolver)
                : base(stringRepresentation)
            {
                _resolver = resolver;
            }

            public IStatBuilder Before(IPoolStatBuilder pool) =>
                CreateStat((IDamageTakenConversionBuilder) this, (IStatBuilder) pool,
                    (o1, o2) => $"{o1} before {o2}");

            public IDamageTakenConversionBuilder Resolve(ResolveContext context) =>
                _resolver(this, context);
        }
    }
}