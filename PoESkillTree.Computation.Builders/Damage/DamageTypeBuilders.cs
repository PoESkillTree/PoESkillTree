using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Damage;

namespace PoESkillTree.Computation.Builders.Damage
{
    public class DamageTypeBuilders : IDamageTypeBuilders
    {
        public DamageTypeBuilders(IStatFactory statFactory)
        {
            Physical = From(DamageType.Physical);
            Fire = From(DamageType.Fire);
            Lightning = From(DamageType.Lightning);
            Cold = From(DamageType.Cold);
            Chaos = From(DamageType.Chaos);
            RandomElement = From(DamageType.RandomElement);

            IDamageTypeBuilder From(DamageType damageType) => new DamageTypeBuilder(statFactory, damageType);
        }

        public IDamageTypeBuilder Physical { get; }
        public IDamageTypeBuilder Fire { get; }
        public IDamageTypeBuilder Lightning { get; }
        public IDamageTypeBuilder Cold { get; }
        public IDamageTypeBuilder Chaos { get; }
        public IDamageTypeBuilder RandomElement { get; }
    }
}