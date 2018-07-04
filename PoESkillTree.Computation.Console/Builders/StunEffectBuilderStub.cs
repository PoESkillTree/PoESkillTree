using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Stats;
using static PoESkillTree.Computation.Console.Builders.BuilderFactory;

namespace PoESkillTree.Computation.Console.Builders
{
    public class StunEffectBuilderStub : AvoidableEffectBuilderStub, IStunEffectBuilder
    {
        public StunEffectBuilderStub()
            : base("Stun", (c, _) => c)
        {
        }

        public IStatBuilder Threshold =>
            CreateStat(This, o => $"{o} threshold");

        public IStatBuilder Recovery =>
            CreateStat(This, o => $"{o} recovery");

        public IStatBuilder ChanceToAvoidInterruptionWhileCasting =>
            CreateStat(This, o => $"Chance to avoid interruption from {o} while casting");
    }
}