using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Entities;
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

        IDamageRelatedStatBuilder IDamageRelatedStatBuilder.For(IEntityBuilder entity) =>
            For(entity);

        public new IDamageStatBuilder For(IEntityBuilder entity) =>
            CreateDamageStat(This, entity, (o1, o2) => $"{o1} for {o2}");

        public IDamageRelatedStatBuilder Taken => CreateDamageStat(This, o => $"{o} taken");

        public IDamageRelatedStatBuilder With(DamageSource source)=>
            CreateDamageStat(This, o1 => $"With {source} {o1}");

        public IDamageRelatedStatBuilder WithHits =>
            CreateDamageStat(This, o => $"With {o} from hits");

        public IDamageRelatedStatBuilder WithHitsAndAilments =>
            CreateDamageStat(This, o => $"With {o} from hits or ailments");

        public IDamageRelatedStatBuilder WithAilments =>
            CreateDamageStat(This, o => $"With {o} from ailments");

        public IDamageRelatedStatBuilder With(IAilmentBuilder ailment) =>
            CreateDamageStat(This, (IEffectBuilder) ailment, (o1, o2) => $"With {o2} {o1}");

        public IDamageRelatedStatBuilder WithSkills =>
            CreateDamageStat(This, o => $"With {o} from skills");

        public IDamageRelatedStatBuilder With(AttackDamageHand hand) =>
            CreateDamageStat(This, o => $"With {hand} {o}");

        public IStatBuilder ApplyModifiersToSkills(DamageSource source, params Form[] forms) =>
            CreateStat(This,
                o => $"Modifiers to {o} apply to source {source} for forms [{string.Join(", ", forms)}]");

        public IStatBuilder ApplyModifiersToAilments(params Form[] forms) =>
            CreateStat(This,
                o => $"Modifiers to {o} apply to ailments for forms [{string.Join(", ", forms)}]");

        public override IStatBuilder WithCondition(IConditionBuilder condition) =>
            CreateDamageStat(This, condition, (s, c) => $"{s} ({c})");
    }
}