using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.Computation.Common.Builders.Stats;
using static PoESkillTree.Computation.Console.Builders.BuilderFactory;

namespace PoESkillTree.Computation.Console.Builders
{
    public class DamageTypeBuilderStub : KeywordBuilderStub, IDamageTypeBuilder
    {
        public DamageTypeBuilderStub(string stringRepresentation, Resolver<IKeywordBuilder> resolver)
            : base(stringRepresentation, resolver)
        {
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

        public IDamageRelatedStatBuilder Penetration =>
            CreateDamageStat(This, o => $"{o} Penetration");

        public IFlagStatBuilder IgnoreResistance =>
            CreateFlagStat(This, o => $"Ignore {o} Resistance");

        public IDamageRelatedStatBuilder ReflectedDamageTaken =>
            CreateDamageStat(This, o => $"Reflected {o} Damage Taken");
    }


    public class DamageTypeBuildersStub : IDamageTypeBuilders
    {
        private static IDamageTypeBuilder Create(string stringRepresentation) =>
            new DamageTypeBuilderStub(stringRepresentation, (current, _) => current);

        public IDamageTypeBuilder Physical => Create("Physical");

        public IDamageTypeBuilder Fire => Create("Fire");

        public IDamageTypeBuilder Lightning => Create("Lightning");

        public IDamageTypeBuilder Cold => Create("Cold");

        public IDamageTypeBuilder Chaos => Create("Chaos");

        public IDamageTypeBuilder RandomElement => Create("Random Element");
    }
}