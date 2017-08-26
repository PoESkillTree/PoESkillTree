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
        protected IConditionBuilders ConditionBuilders { get; }

        public ActionBuilderStub(TSource source, TTarget target, string stringRepresentation,
            IConditionBuilders conditionBuilders) 
            : base(stringRepresentation)
        {
            _source = source;
            _target = target;
            ConditionBuilders = conditionBuilders;
        }

        public IActionBuilder<TNewSource, TTarget> By<TNewSource>(TNewSource source)
            where TNewSource : IEntityBuilder
        {
            return new ActionBuilderStub<TNewSource, TTarget>(source, _target, ToString(),
                ConditionBuilders);
        }

        public IActionBuilder<TSource, TNewTarget> Against<TNewTarget>(TNewTarget target) 
            where TNewTarget : IEntityBuilder
        {
            return new ActionBuilderStub<TSource, TNewTarget>(_source, target, ToString(),
                ConditionBuilders);
        }

        public IActionBuilder<TTarget, TSource> Taken =>
            new ActionBuilderStub<TTarget, TSource>(_target, _source, ToString(),
                ConditionBuilders);

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

        public IConditionBuilder InPastXSeconds(ValueBuilder seconds, Func<TTarget, IConditionBuilder> targetPredicate = null,
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
                new ValueBuilderStub($"Number of {this} recently by {_source} against {_target}"),
                ConditionBuilders);
    }


    public class SelfToAnyActionBuilderStub 
        : ActionBuilderStub<ISelfBuilder, IEntityBuilder>, ISelfToAnyActionBuilder
    {
        public SelfToAnyActionBuilderStub(string stringRepresentation,
            IConditionBuilders conditionBuilders)
            : base(new SelfBuilderStub(conditionBuilders),
                new EntityBuilderStub("Any Entity", conditionBuilders),
                stringRepresentation, conditionBuilders)
        {
        }
    }


    public class BlockActionBuilderStub : SelfToAnyActionBuilderStub, IBlockActionBuilder
    {
        public BlockActionBuilderStub(IConditionBuilders conditionBuilders) 
            : base("Block", conditionBuilders)
        {
        }

        public IStatBuilder Recovery => new StatBuilderStub("Block Recovery", ConditionBuilders);

        public IStatBuilder AttackChance =>
            new StatBuilderStub("Chance to Block Attacks", ConditionBuilders);

        public IStatBuilder SpellChance =>
            new StatBuilderStub("Chance to Block Spells", ConditionBuilders);
    }


    public class CriticalStrikeActionBuilderStub : SelfToAnyActionBuilderStub, 
        ICriticalStrikeActionBuilder
    {
        public CriticalStrikeActionBuilderStub(IConditionBuilders conditionBuilders) 
            : base("Critical Strike", conditionBuilders)
        {
        }

        public IStatBuilder Chance =>
            new StatBuilderStub("Critical Strike Chance", ConditionBuilders);

        public IStatBuilder Multiplier =>
            new StatBuilderStub("Critical Strike Multiplier", ConditionBuilders);

        public IStatBuilder AilmentMultiplier =>
            new StatBuilderStub("Ailment Critical Strike Multipler", ConditionBuilders);

        public IStatBuilder ExtraDamageTaken =>
            new StatBuilderStub("Extra damage taken from Critical Strikes", ConditionBuilders);
    }


    public class ActionBuildersStub : IActionBuilders
    {
        private readonly IConditionBuilders _conditionBuilders;

        public ActionBuildersStub(IConditionBuilders conditionBuilders)
        {
            _conditionBuilders = conditionBuilders;
        }

        public ISelfToAnyActionBuilder Kill =>
            new SelfToAnyActionBuilderStub("Kill", _conditionBuilders);

        public IBlockActionBuilder Block => new BlockActionBuilderStub(_conditionBuilders);

        public ISelfToAnyActionBuilder Hit =>
            new SelfToAnyActionBuilderStub("Hit", _conditionBuilders);

        public ISelfToAnyActionBuilder SavageHit =>
            new SelfToAnyActionBuilderStub("Savage Hit", _conditionBuilders);

        public ICriticalStrikeActionBuilder CriticalStrike =>
            new CriticalStrikeActionBuilderStub(_conditionBuilders);

        public ISelfToAnyActionBuilder NonCriticalStrike =>
            new SelfToAnyActionBuilderStub("Non-critical Strike", _conditionBuilders);

        public ISelfToAnyActionBuilder Shatter =>
            new SelfToAnyActionBuilderStub("Shatter", _conditionBuilders);
        public ISelfToAnyActionBuilder ConsumeCorpse =>
            new SelfToAnyActionBuilderStub("Consuming Corpses", _conditionBuilders);

        public ISelfToAnyActionBuilder SpendMana(ValueBuilder amount) => 
            new SelfToAnyActionBuilderStub($"Spending {amount} mana", _conditionBuilders);
    }
}