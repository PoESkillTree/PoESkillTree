using System.Collections.Generic;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Buffs;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using static PoESkillTree.Computation.Console.Builders.BuilderFactory;

namespace PoESkillTree.Computation.Console.Builders
{
    public class StatBuilderStub : BuilderStub, IStatBuilder
    {
        private readonly Resolver<IStatBuilder> _resolver;

        public StatBuilderStub(string stringRepresentation, Resolver<IStatBuilder> resolver)
            : base(stringRepresentation)
        {
            _resolver = resolver;
        }

        protected IStatBuilder This => this;

        public IStatBuilder Minimum => CreateStat(This, o => $"Minimum {o}");
        public IStatBuilder Maximum => CreateStat(This, o => $"Maximum {o}");

        public ValueBuilder Value => new ValueBuilder(CreateValue(This, o => $"Value of {o}"));

        public IStatBuilder ConvertTo(IStatBuilder stat) =>
            CreateStat(This, stat, (o1, o2) => $"% of {o1} converted to {o2}");

        public IStatBuilder AddAs(IStatBuilder stat) =>
            CreateStat(This, stat, (o1, o2) => $"% of {o1} added as {o2}");

        public IFlagStatBuilder ApplyModifiersTo(IStatBuilder stat,
            IValueBuilder percentOfTheirValue) =>
            CreateFlagStat(This, stat, percentOfTheirValue,
                (o1, o2, o3) => $"Modifiers to {o1} apply to {o2} at {o3}% of their value");

        public IStatBuilder ChanceToDouble =>
            CreateStat(This, o => $"Chance to double {o}");

        public IBuffBuilder ForXSeconds(IValueBuilder seconds) =>
            (IBuffBuilder) Create<IEffectBuilder, IStatBuilder, IValueBuilder>(
                (s, r) => new BuffBuilderStub(s, r),
                This, seconds,
                (o1, o2) => $"{o1} as Buff for {o2} seconds");

        public IBuffBuilder AsBuff =>
            (IBuffBuilder) Create<IEffectBuilder, IStatBuilder>(
                (s, r) => new BuffBuilderStub(s, r),
                This,
                o => $"{o} as Buff");

        public IFlagStatBuilder AsAura(params IEntityBuilder[] affectedEntities) =>
            CreateFlagStat(This, affectedEntities,
                (o1, os) => $"{o1} as Aura affecting [{string.Join(", ", os)}]");

        public IFlagStatBuilder AddTo(ISkillBuilderCollection skills) =>
            CreateFlagStat(This, (IBuilderCollection<ISkillBuilder>) skills,
                (o1, o2) => $"{o1} added to skills {o2}");

        public IFlagStatBuilder AddTo(IEffectBuilder effect) =>
            CreateFlagStat(This, effect, (o1, o2) => $"{o1} added to effect {o2}");

        public virtual IStatBuilder WithCondition(IConditionBuilder condition) =>
            CreateStat(This, condition, (s, c) => $"{s}\n  Condition: {c}");

        public IStatBuilder Resolve(ResolveContext context) =>
            _resolver(this, context);

        public (IReadOnlyList<IStat> stats, ValueConverter valueConverter) Build() =>
            (new[] { new StatStub(this) }, v => v);


        private class StatStub : BuilderStub, IStat
        {
            public StatStub(BuilderStub toCopy) : base(toCopy)
            {
            }
        }
    }


    public class EvasionStatBuilderStub : StatBuilderStub, IEvasionStatBuilder
    {
        public EvasionStatBuilderStub() : base("Evasion", (c, _) => c)
        {
        }

        public IStatBuilder Chance => CreateStat(This, o => $"{o} chance");

        public IStatBuilder ChanceAgainstProjectileAttacks =>
            CreateStat(This, o => $"{o} chance against projectile attacks");

        public IStatBuilder ChanceAgainstMeleeAttacks =>
            CreateStat(This, o => $"{o} chance against melee attacks");
    }
}