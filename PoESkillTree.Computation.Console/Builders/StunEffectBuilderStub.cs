using System;
using PoESkillTree.Computation.Parsing.Builders.Actions;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Effects;
using PoESkillTree.Computation.Parsing.Builders.Entities;
using PoESkillTree.Computation.Parsing.Builders.Skills;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Console.Builders
{
    public class StunEffectBuilderStub : AvoidableEffectBuilderStub, IStunEffectBuilder
    {
        private readonly IActionBuilder<ISelfBuilder, IEnemyBuilder> _actionBuilder;

        public StunEffectBuilderStub() 
            : base("Stun")
        {
            _actionBuilder =
                new ActionBuilderStub<ISelfBuilder, IEnemyBuilder>(
                    new SelfBuilderStub(), 
                    new EnemyBuilderStub(), 
                    "Stun");
        }

        public IStatBuilder Threshold =>
            new StatBuilderStub($"{this} threshold");

        public IStatBuilder Recovery =>
            new StatBuilderStub($"{this} recovery");

        public IStatBuilder ChanceToAvoidInterruptionWhileCasting =>
            new StatBuilderStub($"Chance to avoid interruption from {this} while casting");

        public IConditionBuilder On(IKeywordBuilder withKeyword) => _actionBuilder.On(withKeyword);

        public ValueBuilder CountRecently => _actionBuilder.CountRecently;

        public IActionBuilder<TNewSource, IEnemyBuilder> By<TNewSource>(TNewSource source) 
            where TNewSource : IEntityBuilder
        {
            return _actionBuilder.By(source);
        }

        public IActionBuilder<ISelfBuilder, TNewTarget> Against<TNewTarget>(TNewTarget target) 
            where TNewTarget : IEntityBuilder
        {
            return _actionBuilder.Against(target);
        }

        public IActionBuilder<IEnemyBuilder, ISelfBuilder> Taken => _actionBuilder.Taken;

        public IConditionBuilder On(
            Func<IEnemyBuilder, IConditionBuilder> targetPredicate = null, 
            Func<ISelfBuilder, IConditionBuilder> sourcePredicate = null)
        {
            return _actionBuilder.On(targetPredicate, sourcePredicate);
        }

        public IConditionBuilder InPastXSeconds(ValueBuilder seconds, 
            Func<IEnemyBuilder, IConditionBuilder> targetPredicate = null,
            Func<ISelfBuilder, IConditionBuilder> sourcePredicate = null)
        {
            return _actionBuilder.InPastXSeconds(seconds, targetPredicate, sourcePredicate);
        }

        public IConditionBuilder Recently(
            Func<IEnemyBuilder, IConditionBuilder> targetPredicate = null,
            Func<ISelfBuilder, IConditionBuilder> sourcePredicate = null)
        {
            return _actionBuilder.Recently(targetPredicate, sourcePredicate);
        }
    }
}