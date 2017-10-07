using System;
using PoESkillTree.Computation.Parsing.Builders.Actions;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Entities;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Builders.Skills;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Builders.Values;
using static PoESkillTree.Computation.Console.Builders.BuilderFactory;

namespace PoESkillTree.Computation.Console.Builders
{
    public class ActionBuilderStub<TSource, TTarget> 
        : BuilderStub, IActionBuilder<TSource, TTarget>
        where TSource : IEntityBuilder
        where TTarget : IEntityBuilder
    {
        private readonly TSource _source;
        private readonly TTarget _target;

        private readonly Resolver<IActionBuilder> _resolver;

        public ActionBuilderStub(TSource source, TTarget target, string stringRepresentation, 
            Resolver<IActionBuilder> resolver) 
            : base(stringRepresentation)
        {
            _source = source;
            _target = target;
            _resolver = resolver;
        }

        private ActionBuilderStub(TSource source, TTarget target, string stringRepresentation)
            : this(source, target, stringRepresentation, Resolve)
        {
        }

        private static IActionBuilder Resolve(
            IActionBuilder current,
            IMatchContext<IValueBuilder> valueContext)
        {
            return new ActionBuilderStub<IEntityBuilder, IEntityBuilder>(
                current.Source.Resolve(valueContext),
                current.Target.Resolve(valueContext),
                current.ToString(),
                (c, _) => c);
        }

        public IEntityBuilder Source => _source;

        public IEntityBuilder Target => _target;

        private IActionBuilder This => this;

        public IActionBuilder<TNewSource, TTarget> By<TNewSource>(TNewSource source)
            where TNewSource : IEntityBuilder
        {
            return new ActionBuilderStub<TNewSource, TTarget>(source, _target, ToString());
        }

        public IActionBuilder<TSource, TNewTarget> Against<TNewTarget>(TNewTarget target) 
            where TNewTarget : IEntityBuilder
        {
            return new ActionBuilderStub<TSource, TNewTarget>(_source, target, ToString());
        }

        public IActionBuilder<TTarget, TSource> Taken =>
            new ActionBuilderStub<TTarget, TSource>(_target, _source, ToString());

        public IConditionBuilder On(IKeywordBuilder withKeyword) =>
            CreateCondition(This, withKeyword,
                (a, keword) => $"On {keword} {a} by {a.Source} against {a.Target}");

        public IConditionBuilder On(
            Func<TTarget, IConditionBuilder> targetPredicate = null,
            Func<TSource, IConditionBuilder> sourcePredicate = null)
        {
            string StringRepresentation(IActionBuilder current, IConditionBuilder sourceCond,
                IConditionBuilder targetCond) =>
                $"On {current}" +
                $" by {current.Source}{ConditionToString(sourceCond)}" +
                $" against {current.Target}{ConditionToString(targetCond)}";

            var sourceCondition = sourcePredicate?.Invoke(_source);
            var targetCondition = targetPredicate?.Invoke(_target);

            return CreateCondition(This, sourceCondition, targetCondition, StringRepresentation);
        }

        public IConditionBuilder InPastXSeconds(
            IValueBuilder seconds, 
            Func<TTarget, IConditionBuilder> targetPredicate = null,
            Func<TSource, IConditionBuilder> sourcePredicate = null)
        {
            string StringRepresentation(IActionBuilder current, IValueBuilder secondsValue,
                IConditionBuilder sourceCond, IConditionBuilder targetCond) => 
                $"If any {current} in the past {secondsValue}" +
                $" by {current.Source}{ConditionToString(sourceCond)}" +
                $" against {current.Target}{ConditionToString(targetCond)}";

            var sourceCondition = sourcePredicate?.Invoke(_source);
            var targetCondition = targetPredicate?.Invoke(_target);

            return CreateCondition(This, seconds, sourceCondition, targetCondition,
                StringRepresentation);
        }

        public IConditionBuilder Recently(
            Func<TTarget, IConditionBuilder> targetPredicate = null, 
            Func<TSource, IConditionBuilder> sourcePredicate = null)
        {
            string StringRepresentation(IActionBuilder current, IConditionBuilder sourceCond,
                IConditionBuilder targetCond) =>
                $"If any {current} recently" +
                $" by {current.Source}{ConditionToString(sourceCond)}" +
                $" against {current.Target}{ConditionToString(targetCond)}";

            var sourceCondition = sourcePredicate?.Invoke(_source);
            var targetCondition = targetPredicate?.Invoke(_target);

            return CreateCondition(This, sourceCondition, targetCondition, StringRepresentation);
        }

        private static string ConditionToString(IConditionBuilder condition) =>
            condition == null ? "" : $"({condition}";

        public ValueBuilder CountRecently =>
            new ValueBuilder(
                CreateValue($"Number of {this} recently by {Source} against {Target}"));

        public IActionBuilder Resolve(IMatchContext<IValueBuilder> valueContext) => 
            _resolver(this, valueContext);
    }


    public class SelfToAnyActionBuilderStub 
        : ActionBuilderStub<ISelfBuilder, IEntityBuilder>, ISelfToAnyActionBuilder
    {
        public SelfToAnyActionBuilderStub(string stringRepresentation, 
            Resolver<IActionBuilder> resolver)
            : base(new SelfBuilderStub(),
                new EntityBuilderStub("Any Entity", (c, _) => c),
                stringRepresentation,
                resolver)
        {
        }
    }


    public class BlockActionBuilderStub : SelfToAnyActionBuilderStub, IBlockActionBuilder
    {
        public BlockActionBuilderStub() 
            : base("Block", (current, _) => current)
        {
        }

        public IStatBuilder Recovery => CreateStat("Block Recovery");

        public IStatBuilder AttackChance => CreateStat("Chance to Block Attacks");

        public IStatBuilder SpellChance => CreateStat("Chance to Block Spells");
    }


    public class CriticalStrikeActionBuilderStub : SelfToAnyActionBuilderStub, 
        ICriticalStrikeActionBuilder
    {
        public CriticalStrikeActionBuilderStub() 
            : base("Critical Strike", (current, _) => current)
        {
        }

        public IStatBuilder Chance => CreateStat("Critical Strike Chance");

        public IStatBuilder Multiplier => CreateStat("Critical Strike Multiplier");

        public IStatBuilder AilmentMultiplier => CreateStat("Ailment Critical Strike Multipler");

        public IStatBuilder ExtraDamageTaken =>
            CreateStat("Extra damage taken from Critical Strikes");
    }


    public class ActionBuildersStub : IActionBuilders
    {
        private static ISelfToAnyActionBuilder Create(string stringRepresentation) =>
            new SelfToAnyActionBuilderStub(stringRepresentation, (current, _) => current);

        public ISelfToAnyActionBuilder Kill => Create("Kill");

        public IBlockActionBuilder Block => new BlockActionBuilderStub();

        public ISelfToAnyActionBuilder Hit => Create("Hit");

        public ISelfToAnyActionBuilder SavageHit => Create("Savage Hit");

        public ICriticalStrikeActionBuilder CriticalStrike =>
            new CriticalStrikeActionBuilderStub();

        public ISelfToAnyActionBuilder NonCriticalStrike => Create("Non-critical Strike");

        public ISelfToAnyActionBuilder Shatter => Create("Shatter");
        public ISelfToAnyActionBuilder ConsumeCorpse => Create("Consuming Corpses");

        public ISelfToAnyActionBuilder SpendMana(IValueBuilder amount) => 
            (ISelfToAnyActionBuilder) Create<IActionBuilder, IValueBuilder>(
                (s, r) => new SelfToAnyActionBuilderStub(s, r),
                amount, 
                o => $"Spending {o} mana");
    }
}