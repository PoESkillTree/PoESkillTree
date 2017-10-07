using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Damage;
using PoESkillTree.Computation.Parsing.Builders.Effects;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Console.Builders
{
    public class DamageStatBuilderStub : StatBuilderStub, IDamageStatBuilder
    {
        public DamageStatBuilderStub(string stringRepresentation, Resolver<IStatBuilder> resolver) 
            : base(stringRepresentation, resolver)
        {
        }

        public IStatBuilder Taken => BuilderFactory.CreateStat(This, o => $"{o} taken");

        public IDamageTakenConversionBuilder TakenFrom(IPoolStatBuilder pool) =>
            BuilderFactory.Create<IDamageTakenConversionBuilder, IStatBuilder, IStatBuilder>(
                (s, r) => new DamageTakenConversionBuilder(s, r),
                This, pool,
                (o1, o2) => $"{o1} taken from {o2}");

        public IConditionBuilder With() =>
            BuilderFactory.CreateCondition(This, o => $"With {o}");

        public IConditionBuilder With(IDamageSourceBuilder source) =>
            BuilderFactory.CreateCondition(This, source, (o1, o2) => $"With {o2} {o1}");

        public IConditionBuilder With(Tags tags) =>
            BuilderFactory.CreateCondition(This, o => $"With {tags} {o}");

        public IConditionBuilder With(IAilmentBuilder ailment) =>
            BuilderFactory.CreateCondition(This, (IEffectBuilder) ailment, (o1, o2) => $"With {o2} {o1}");

        public IConditionBuilder With(ItemSlot slot) =>
            BuilderFactory.CreateCondition(This, o => $"With {slot} {o}");


        private class DamageTakenConversionBuilder : BuilderStub, IDamageTakenConversionBuilder
        {
            private readonly Resolver<IDamageTakenConversionBuilder> _resolver;

            public DamageTakenConversionBuilder(string stringRepresentation,
                Resolver<IDamageTakenConversionBuilder> resolver) 
                : base(stringRepresentation)
            {
                _resolver = resolver;
            }

            public IStatBuilder Before(IPoolStatBuilder pool) =>
                BuilderFactory.CreateStat((IDamageTakenConversionBuilder) this, (IStatBuilder) pool,
                    (o1, o2) => $"{o1} before {o2}");

            public IDamageTakenConversionBuilder
                Resolve(IMatchContext<IValueBuilder> valueContext) => _resolver(this, valueContext);
        }
    }
}