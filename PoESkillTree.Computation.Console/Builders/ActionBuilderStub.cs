using System;
using PoESkillTree.Computation.Parsing.Builders.Actions;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Entities;
using PoESkillTree.Computation.Parsing.Builders.Skills;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Console.Builders
{
    public class ActionBuilderStub<TSource, TTarget> : BuilderStub, 
        IActionBuilder<TSource, TTarget>
        where TSource : IEntityBuilder
        where TTarget : IEntityBuilder
    {
        private readonly TSource _source;
        private readonly TTarget _target;

        public ActionBuilderStub(TSource source, TTarget target, string stringRepresentation) 
            : base(stringRepresentation)
        {
            _source = source;
            _target = target;
        }

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
            new ConditionBuilderStub($"On {withKeyword} {this} by {_source} against {_target}");

        public IConditionBuilder On(Func<TTarget, IConditionBuilder> targetPredicate = null,
            Func<TSource, IConditionBuilder> sourcePredicate = null)
        {
            var sourceCondition = SourcePredicateToString(sourcePredicate);
            var targetCondition = TargetPredicateToString(targetPredicate);
            return new ConditionBuilderStub(
                $"On {this} by {_source}{sourceCondition} against {_target}{targetCondition}");
        }

        public IConditionBuilder InPastXSeconds(ValueBuilder seconds, 
            Func<TTarget, IConditionBuilder> targetPredicate = null,
            Func<TSource, IConditionBuilder> sourcePredicate = null)
        {
            var sourceCondition = SourcePredicateToString(sourcePredicate);
            var targetCondition = TargetPredicateToString(targetPredicate);
            return new ConditionBuilderStub(
                $"If any {this} in the past {seconds} seconds by {_source}{sourceCondition} " +
                $"against {_target}{targetCondition}");
        }

        public IConditionBuilder Recently(Func<TTarget, IConditionBuilder> targetPredicate = null, 
            Func<TSource, IConditionBuilder> sourcePredicate = null)
        {
            var sourceCondition = SourcePredicateToString(sourcePredicate);
            var targetCondition = TargetPredicateToString(targetPredicate);
            return new ConditionBuilderStub(
                $"If any {this} recently by {_source}{sourceCondition} " +
                $"against {_target}{targetCondition}");
        }

        private string SourcePredicateToString(Func<TSource, IConditionBuilder> predicate) => 
            PredicateToString(predicate, _source);

        private string TargetPredicateToString(Func<TTarget, IConditionBuilder> predicate) =>
            PredicateToString(predicate, _target);

        private static string PredicateToString<T>(Func<T, IConditionBuilder> predicate, T entity)
        {
            if (predicate == null)
            {
                return "";
            }
            return " (" + predicate(entity) + ")";
        }

        public ValueBuilder CountRecently =>
            new ValueBuilder(
                new ValueBuilderStub($"Number of {this} recently by {_source} against {_target}"));
    }


    public class SelfToAnyActionBuilderStub 
        : ActionBuilderStub<ISelfBuilder, IEntityBuilder>, ISelfToAnyActionBuilder
    {
        public SelfToAnyActionBuilderStub(string stringRepresentation)
            : base(new SelfBuilderStub(),
                new EntityBuilderStub("Any Entity"),
                stringRepresentation)
        {
        }
    }


    public class BlockActionBuilderStub : SelfToAnyActionBuilderStub, IBlockActionBuilder
    {
        public BlockActionBuilderStub() 
            : base("Block")
        {
        }

        public IStatBuilder Recovery => new StatBuilderStub("Block Recovery");

        public IStatBuilder AttackChance =>
            new StatBuilderStub("Chance to Block Attacks");

        public IStatBuilder SpellChance =>
            new StatBuilderStub("Chance to Block Spells");
    }


    public class CriticalStrikeActionBuilderStub : SelfToAnyActionBuilderStub, 
        ICriticalStrikeActionBuilder
    {
        public CriticalStrikeActionBuilderStub() 
            : base("Critical Strike")
        {
        }

        public IStatBuilder Chance =>
            new StatBuilderStub("Critical Strike Chance");

        public IStatBuilder Multiplier =>
            new StatBuilderStub("Critical Strike Multiplier");

        public IStatBuilder AilmentMultiplier =>
            new StatBuilderStub("Ailment Critical Strike Multipler");

        public IStatBuilder ExtraDamageTaken =>
            new StatBuilderStub("Extra damage taken from Critical Strikes");
    }


    public class ActionBuildersStub : IActionBuilders
    {
        public ISelfToAnyActionBuilder Kill =>
            new SelfToAnyActionBuilderStub("Kill");

        public IBlockActionBuilder Block => new BlockActionBuilderStub();

        public ISelfToAnyActionBuilder Hit =>
            new SelfToAnyActionBuilderStub("Hit");

        public ISelfToAnyActionBuilder SavageHit =>
            new SelfToAnyActionBuilderStub("Savage Hit");

        public ICriticalStrikeActionBuilder CriticalStrike =>
            new CriticalStrikeActionBuilderStub();

        public ISelfToAnyActionBuilder NonCriticalStrike =>
            new SelfToAnyActionBuilderStub("Non-critical Strike");

        public ISelfToAnyActionBuilder Shatter =>
            new SelfToAnyActionBuilderStub("Shatter");
        public ISelfToAnyActionBuilder ConsumeCorpse =>
            new SelfToAnyActionBuilderStub("Consuming Corpses");

        public ISelfToAnyActionBuilder SpendMana(ValueBuilder amount) => 
            new SelfToAnyActionBuilderStub($"Spending {amount} mana");
    }
}