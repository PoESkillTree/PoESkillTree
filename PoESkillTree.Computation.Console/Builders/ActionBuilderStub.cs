using PoESkillTree.Computation.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Actions;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using static PoESkillTree.Computation.Console.Builders.BuilderFactory;

namespace PoESkillTree.Computation.Console.Builders
{
    public class ActionBuilderStub : BuilderStub, IActionBuilder
    {
        private readonly Resolver<IActionBuilder> _resolver;

        public ActionBuilderStub(IEntityBuilder source, string stringRepresentation,
            Resolver<IActionBuilder> resolver)
            : base(stringRepresentation)
        {
            Source = source;
            _resolver = resolver;
        }

        public static IActionBuilder BySelf(string stringRepresentation, Resolver<IActionBuilder> resolver) =>
            new ActionBuilderStub(new EntityBuilder(), stringRepresentation, resolver);

        public IEntityBuilder Source { get; }

        private IActionBuilder This => this;

        public IActionBuilder By(IEntityBuilder source)
        {
            IActionBuilder Resolve(ResolveContext context)
            {
                var inner = _resolver(this, context);
                return new ActionBuilderStub(
                    source.Resolve(context),
                    inner.ToString(),
                    (c, _) => c);
            }

            return new ActionBuilderStub(source, ToString(), (_, context) => Resolve(context));
        }

        public IActionBuilder With(IDamageTypeBuilder damageType)
        {
            IActionBuilder Resolve(ResolveContext context)
            {
                var inner = _resolver(this, context);
                return new ActionBuilderStub(
                    inner.Source,
                    $"{inner} (with {damageType.Resolve(context)} damage)",
                    (c, _) => c);
            }

            return new ActionBuilderStub(
                Source, $"{this} (with {damageType} damage)", (_, context) => Resolve(context));
        }

        public IConditionBuilder On() =>
            CreateCondition(This,
                a => $"On {a} by {a.Source}");

        public IConditionBuilder On(IKeywordBuilder withKeyword) =>
            CreateCondition(This, withKeyword,
                (a, keyword) => $"On {keyword} {a} by {a.Source}");

        public IConditionBuilder InPastXSeconds(IValueBuilder seconds) =>
            CreateCondition(This, seconds,
                (a, o) => $"If any {a} in the past {o} by {a.Source}");

        public IConditionBuilder Recently =>
            CreateCondition(This,
                a => $"If any {a} recently by {a.Source}");

        public ValueBuilder CountRecently =>
            new ValueBuilder(CreateValue($"Number of {this} recently by {Source}"));

        public IActionBuilder Resolve(ResolveContext context) => _resolver(this, context);
    }


    public class BlockActionBuilderStub : ActionBuilderStub, IBlockActionBuilder
    {
        public BlockActionBuilderStub()
            : base(new EntityBuilder(), "Block", (current, _) => current)
        {
        }

        public IStatBuilder Recovery => CreateStat("Block Recovery");

        public IStatBuilder AttackChance => CreateStat("Chance to Block Attacks");

        public IStatBuilder SpellChance => CreateStat("Chance to Block Spells");
    }


    public class CriticalStrikeActionBuilderStub : ActionBuilderStub, ICriticalStrikeActionBuilder
    {
        public CriticalStrikeActionBuilderStub()
            : base(new EntityBuilder(), "Critical Strike", (current, _) => current)
        {
        }

        public IDamageRelatedStatBuilder Chance => CreateDamageStat("Critical Strike Chance");

        public IDamageRelatedStatBuilder Multiplier => CreateDamageStat("Critical Strike Multiplier");

        public IStatBuilder ExtraDamageTaken => CreateStat("Extra damage taken from Critical Strikes");
    }


    public class ActionBuildersStub : IActionBuilders
    {
        private static IActionBuilder Create(string stringRepresentation) =>
            ActionBuilderStub.BySelf(stringRepresentation, (current, _) => current);

        public IActionBuilder Kill => Create("Kill");

        public IBlockActionBuilder Block => new BlockActionBuilderStub();

        public IActionBuilder Hit => Create("Hit");

        public IActionBuilder SavageHit => Create("Savage Hit");

        public ICriticalStrikeActionBuilder CriticalStrike => new CriticalStrikeActionBuilderStub();

        public IActionBuilder NonCriticalStrike => Create("Non-critical Strike");

        public IActionBuilder Shatter => Create("Shatter");
        public IActionBuilder ConsumeCorpse => Create("Consuming Corpses");

        public IActionBuilder SpendMana(IValueBuilder amount) =>
            Create<IActionBuilder, IValueBuilder>(ActionBuilderStub.BySelf, amount, o => $"Spending {o} Mana");

        public IActionBuilder Unique(string description) => Create(description);

        public IConditionBuilder InPastXSeconds(IValueBuilder seconds) =>
            CreateCondition(seconds, o => $"If the action condition happened in the past {o} seconds");
    }
}