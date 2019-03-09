using System.Collections.Generic;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.GameModel;

namespace PoESkillTree.Computation.Data.GivenStats
{
    public class AllyGivenStats : LevelBasedStats
    {
        public AllyGivenStats(
            IBuilderFactories builderFactories, IModifierBuilder modifierBuilder, MonsterBaseStats monsterBaseStats)
            : base(builderFactories, modifierBuilder, monsterBaseStats)
        {
        }

        public override IReadOnlyList<Entity> AffectedEntities { get; } =
            new[] { GameModel.Entity.Totem, GameModel.Entity.Minion };

        protected override GivenStatCollection CreateCollection()
            => new GivenStatCollection(ModifierBuilder, ValueFactory)
            {
                { BaseAdd, Ground.Consecrated.AddStat(Life.Regen), 6 },
                // Level based
                { BaseSet, Life, LevelBased(l => MonsterBaseStats.AllyLife(l), "AllyLife") },
            };
    }
}