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

        public ActionBuilderStub(IEntityBuilder source, IEntityBuilder target, string stringRepresentation,
            Resolver<IActionBuilder> resolver)
            : base(stringRepresentation)
        {
            Source = source;
            Target = target;
            _resolver = resolver;
        }

        public static IActionBuilder SelfToAny(string stringRepresentation, Resolver<IActionBuilder> resolver) =>
            new ActionBuilderStub(EntityBuilderStub.Self(), EntityBuilderStub.Any(), stringRepresentation, resolver);

        public IEntityBuilder Source { get; }

        public IEntityBuilder Target { get; }

        private IActionBuilder This => this;

        public IActionBuilder By(IEntityBuilder source)
        {
            IActionBuilder Resolve(ResolveContext context)
            {
                var inner = _resolver(this, context);
                return new ActionBuilderStub(
                    source.Resolve(context),
                    inner.Target,
                    inner.ToString(),
                    (c, _) => c);
            }

            return new ActionBuilderStub(source, Target, ToString(), (_, context) => Resolve(context));
        }

        public IActionBuilder Against(IEntityBuilder target)
        {
            IActionBuilder Resolve(ResolveContext context)
            {
                var inner = _resolver(this, context);
                return new ActionBuilderStub(
                    inner.Source,
                    target.Resolve(context),
                    inner.ToString(),
                    (c, _) => c);
            }

            return new ActionBuilderStub(Source, target, ToString(), (_, context) => Resolve(context));
        }

        public IActionBuilder Taken
        {
            get
            {
                IActionBuilder Resolve(ResolveContext context)
                {
                    var inner = _resolver(this, context);
                    return new ActionBuilderStub(
                        inner.Target,
                        inner.Source,
                        inner.ToString(),
                        (c, _) => c);
                }

                return new ActionBuilderStub(Target, Source, ToString(), (_, context) => Resolve(context));
            }
        }

        public IActionBuilder With(IDamageTypeBuilder damageType)
        {
            IActionBuilder Resolve(ResolveContext context)
            {
                var inner = _resolver(this, context);
                return new ActionBuilderStub(
                    inner.Source,
                    inner.Target,
                    $"{inner} (with {damageType.Resolve(context)} damage)",
                    (c, _) => c);
            }

            return new ActionBuilderStub(
                Source, Target, $"{this} (with {damageType} damage)", (_, context) => Resolve(context));
        }

        public IConditionBuilder On() =>
            CreateCondition(This,
                a => $"On {a} by {a.Source} against {a.Target}");

        public IConditionBuilder On(IKeywordBuilder withKeyword) =>
            CreateCondition(This, withKeyword,
                (a, keyword) => $"On {keyword} {a} by {a.Source} against {a.Target}");

        public IConditionBuilder InPastXSeconds(IValueBuilder seconds) =>
            CreateCondition(This, seconds,
                (a, o) => $"If any {a} in the past {o} by {a.Source} against {a.Target}");

        public IConditionBuilder Recently =>
            CreateCondition(This,
                a => $"If any {a} recently by {a.Source} against {a.Target}");

        public ValueBuilder CountRecently =>
            new ValueBuilder(CreateValue($"Number of {this} recently by {Source} against {Target}"));

        public IActionBuilder Resolve(ResolveContext context) => _resolver(this, context);
    }


    public class BlockActionBuilderStub : ActionBuilderStub, IBlockActionBuilder
    {
        public BlockActionBuilderStub()
            : base(EntityBuilderStub.Self(), EntityBuilderStub.Any(), "Block", (current, _) => current)
        {
        }

        public IStatBuilder Recovery => CreateStat("Block Recovery");

        public IStatBuilder AttackChance => CreateStat("Chance to Block Attacks");

        public IStatBuilder SpellChance => CreateStat("Chance to Block Spells");
    }


    public class CriticalStrikeActionBuilderStub : ActionBuilderStub, ICriticalStrikeActionBuilder
    {
        public CriticalStrikeActionBuilderStub()
            : base(EntityBuilderStub.Self(), EntityBuilderStub.Any(), "Critical Strike", (current, _) => current)
        {
        }

        public IStatBuilder Chance => CreateStat("Critical Strike Chance");

        public IStatBuilder Multiplier => CreateStat("Critical Strike Multiplier");

        public IStatBuilder AilmentMultiplier => CreateStat("Ailment Critical Strike Multiplier");

        public IStatBuilder ExtraDamageTaken => CreateStat("Extra damage taken from Critical Strikes");
    }


    public class ActionBuildersStub : IActionBuilders
    {
        private static IActionBuilder Create(string stringRepresentation) =>
            ActionBuilderStub.SelfToAny(stringRepresentation, (current, _) => current);

        public IActionBuilder Kill => Create("Kill");

        public IBlockActionBuilder Block => new BlockActionBuilderStub();

        public IActionBuilder Hit => Create("Hit");

        public IActionBuilder SavageHit => Create("Savage Hit");

        public ICriticalStrikeActionBuilder CriticalStrike => new CriticalStrikeActionBuilderStub();

        public IActionBuilder NonCriticalStrike => Create("Non-critical Strike");

        public IActionBuilder Shatter => Create("Shatter");
        public IActionBuilder ConsumeCorpse => Create("Consuming Corpses");

        public IActionBuilder SpendMana(IValueBuilder amount) =>
            Create<IActionBuilder, IValueBuilder>(ActionBuilderStub.SelfToAny, amount, o => $"Spending {o} Mana");

        public IConditionBuilder InPastXSeconds(IValueBuilder seconds) =>
            CreateCondition(seconds, o => $"If the action condition happened in the past {o} seconds");
    }
}