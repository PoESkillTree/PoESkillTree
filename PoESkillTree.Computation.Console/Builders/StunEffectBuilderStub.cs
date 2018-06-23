using PoESkillTree.Computation.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Actions;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
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
                    new ModifierSourceEntityBuilder(), 
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

        public IActionBuilder By(IEntityBuilder source) => _actionBuilder.By(source);

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