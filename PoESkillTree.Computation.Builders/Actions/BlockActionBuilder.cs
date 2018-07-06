using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Actions;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Builders.Actions
{
    internal class BlockActionBuilder : ActionBuilder, IBlockActionBuilder
    {
        public BlockActionBuilder(IStatFactory statFactory, IEntityBuilder entity)
            : base(statFactory, new ConstantStringBuilder("Block"), entity)
        {
        }

        public IStatBuilder Recovery =>
            StatBuilderUtils.FromIdentity(StatFactory, $"{BuildIdentity()}.Recovery", typeof(int));

        public IStatBuilder AttackChance =>
            StatBuilderUtils.FromIdentity(StatFactory, $"{BuildIdentity()}.ChanceAgainstAttacks", typeof(int));

        public IStatBuilder SpellChance =>
            StatBuilderUtils.FromIdentity(StatFactory, $"{BuildIdentity()}.ChanceAgainstSpells", typeof(int));
    }
}