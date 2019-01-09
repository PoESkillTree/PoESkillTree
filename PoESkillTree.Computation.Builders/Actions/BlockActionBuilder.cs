using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Actions;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Builders.Actions
{
    internal class BlockActionBuilder : ActionBuilder, IBlockActionBuilder
    {
        public BlockActionBuilder(IStatFactory statFactory, IEntityBuilder entity)
            : base(statFactory, CoreBuilder.Create("Block"), entity)
        {
        }

        public IStatBuilder Recovery =>
            StatBuilderUtils.FromIdentity(StatFactory, "Block.Recovery", typeof(int));

        public IStatBuilder AttackChance =>
            StatBuilderUtils.FromIdentity(StatFactory, "Block.ChanceAgainstAttacks", typeof(uint));

        public IStatBuilder SpellChance =>
            StatBuilderUtils.FromIdentity(StatFactory, "Block.ChanceAgainstSpells", typeof(uint));
    }
}