using PoESkillTree.Computation.Common.Builders.Damage;

namespace PoESkillTree.Computation.Builders.Damage
{
    public class DamageSourceBuilders : IDamageSourceBuilders
    {
        public IDamageSourceBuilder From(DamageSource source) => new Builder(source);

        private class Builder : ConstantBuilder<IDamageSourceBuilder, DamageSource>, IDamageSourceBuilder
        {
            public Builder(DamageSource source) : base(source)
            {
            }
        }
    }
}