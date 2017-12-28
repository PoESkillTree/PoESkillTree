using PoESkillTree.Computation.Parsing.Builders.Actions;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Damage;
using PoESkillTree.Computation.Parsing.Builders.Effects;
using PoESkillTree.Computation.Parsing.Builders.Entities;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Builders.Skills;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Builders.Values;
using static PoESkillTree.Computation.Console.Builders.BuilderFactory;

namespace PoESkillTree.Computation.Console.Builders
{
    public class StunEffectBuilderStub : AvoidableEffectBuilderStub, IStunEffectBuilder
    {
        private readonly IActionBuilder _actionBuilder;

        public StunEffectBuilderStub() 
            : base("Stun", (c, _) => c)
        {
            _actionBuilder =
                new ActionBuilderStub(
                    EntityBuilderStub.Self(), 
                    new EnemyBuilderStub(), 
                    "Stun", 
                    (c, _) => c);
        }

        public IStatBuilder Threshold =>
            CreateStat(This, o => $"{o} threshold");

        public IStatBuilder Recovery =>
            CreateStat(This, o => $"{o} recovery");

        public IStatBuilder ChanceToAvoidInterruptionWhileCasting =>
            CreateStat(This, o => $"Chance to avoid interruption from {o} while casting");

        public IEntityBuilder Source => _actionBuilder.Source;

        public IEntityBuilder Target => _actionBuilder.Target;

        public IActionBuilder By(IEntityBuilder source) => _actionBuilder.By(source);

        public IActionBuilder Against(IEntityBuilder target) => _actionBuilder.Against(target);

        public IActionBuilder Taken => _actionBuilder.Taken;

        public IActionBuilder With(IDamageTypeBuilder damageType) => _actionBuilder.With(damageType);

        public IConditionBuilder On() => _actionBuilder.On();

        public IConditionBuilder On(IKeywordBuilder withKeyword) => _actionBuilder.On(withKeyword);

        public IConditionBuilder InPastXSeconds(IValueBuilder seconds) => _actionBuilder.InPastXSeconds(seconds);

        public IConditionBuilder Recently => _actionBuilder.Recently;

        public ValueBuilder CountRecently => _actionBuilder.CountRecently;

        IActionBuilder IResolvable<IActionBuilder>.Resolve(ResolveContext context)
        {
            return this;
        }
    }
}