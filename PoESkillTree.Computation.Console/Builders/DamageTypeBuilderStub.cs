using System.Collections.Generic;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.Computation.Common.Builders.Stats;
using static PoESkillTree.Computation.Console.Builders.BuilderFactory;

namespace PoESkillTree.Computation.Console.Builders
{
    public class DamageTypeBuilderStub : BuilderStub, IDamageTypeBuilder
    {
        private readonly Resolver<IKeywordBuilder> _resolver;

        public DamageTypeBuilderStub(string stringRepresentation, Resolver<IKeywordBuilder> resolver)
            : base(stringRepresentation)
        {
            _resolver = resolver;
        }

        private IKeywordBuilder This => this;

        private static IKeywordBuilder Construct(string stringRepresentation, Resolver<IKeywordBuilder> resolver) =>
            new DamageTypeBuilderStub(stringRepresentation, resolver);

        public IDamageTypeBuilder And(IDamageTypeBuilder type) =>
            (IDamageTypeBuilder) Create(
                Construct, This, (IKeywordBuilder) type,
                (l, r) => $"{l}, {r}");

        public IDamageTypeBuilder Invert =>
            (IDamageTypeBuilder) Create(
                Construct, This,
                o => $"Invert({o})");

        public IDamageTypeBuilder Except(IDamageTypeBuilder type) =>
            (IDamageTypeBuilder) Create(
                Construct, This, (IKeywordBuilder) type,
                (l, r) => $"({l}).Except({r})");

        public IStatBuilder Resistance =>
            CreateStat(This, o => $"{o} Resistance");

        public IDamageStatBuilder Damage =>
            CreateDamageStat(This, o => $"{o} Damage");

        public IDamageTakenConversionBuilder DamageTakenFrom(IPoolStatBuilder pool) =>
            Create<DamageTakenConversionBuilder, IKeywordBuilder, IStatBuilder>(
                (s, r) => new DamageTakenConversionBuilder(s, r),
                This, pool,
                (o1, o2) => $"{o1} taken from {o2}");

        public IDamageRelatedStatBuilder Penetration =>
            CreateDamageStat(This, o => $"{o} Penetration");

        public IFlagStatBuilder IgnoreResistance =>
            CreateFlagStat(This, o => $"Ignore {o} Resistance");

        public IStatBuilder ReflectedDamageTaken =>
            CreateDamageStat(This, o => $"Reflected {o} Damage Taken");

        public IKeywordBuilder Resolve(ResolveContext context) => _resolver(this, context);

        public Keyword Build() => Keyword.Projectile;

        public IReadOnlyList<DamageType> BuildDamageTypes() => new DamageType[0];


        private class DamageTakenConversionBuilder : BuilderStub, IDamageTakenConversionBuilder,
            IResolvable<DamageTakenConversionBuilder>
        {
            private readonly Resolver<DamageTakenConversionBuilder> _resolver;

            public DamageTakenConversionBuilder(
                string stringRepresentation, Resolver<DamageTakenConversionBuilder> resolver)
                : base(stringRepresentation)
            {
                _resolver = resolver;
            }

            public IStatBuilder Before(IPoolStatBuilder pool) =>
                CreateStat(this, (IStatBuilder) pool,
                    (o1, o2) => $"{o1} before {o2}");

            public DamageTakenConversionBuilder Resolve(ResolveContext context) =>
                _resolver(this, context);
        }
    }
}