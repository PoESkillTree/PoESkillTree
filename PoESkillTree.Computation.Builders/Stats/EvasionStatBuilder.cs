using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Builders.Stats
{
    public class EvasionStatBuilder : StatBuilder, IEvasionStatBuilder
    {
        private const string Prefix = "Evasion";

        public EvasionStatBuilder(IStatFactory statFactory)
            : base(statFactory, LeafCoreStatBuilder.FromIdentity(statFactory, Prefix, typeof(int)))
        {
        }

        public IStatBuilder Chance => ChanceAgainstProjectileAttacks.CombineWith(ChanceAgainstMeleeAttacks);

        public IStatBuilder ChanceAgainstProjectileAttacks =>
            FromIdentity($"{Prefix} chance against projectile attacks", typeof(double));

        public IStatBuilder ChanceAgainstMeleeAttacks =>
            FromIdentity($"{Prefix} chance against melee attacks", typeof(double));
    }
}