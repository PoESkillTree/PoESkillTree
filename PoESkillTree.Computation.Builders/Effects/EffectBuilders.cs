using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Effects;

namespace PoESkillTree.Computation.Builders.Effects
{
    internal class EffectBuilders : IEffectBuilders
    {
        public EffectBuilders(IStatFactory statFactory)
        {
            Stun = new StunEffectBuilder(statFactory);
            Knockback = new KnockbackEffectBuilder(statFactory);
            Ailment = new AilmentBuilders(statFactory);
            Ground = new GroundEffectBuilders(statFactory);
        }

        public IStunEffectBuilder Stun { get; }
        public IKnockbackEffectBuilder Knockback { get; }
        public IAilmentBuilders Ailment { get; }
        public IGroundEffectBuilders Ground { get; }
    }
}