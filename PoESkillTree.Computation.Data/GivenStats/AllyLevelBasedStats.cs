using System.Collections.Generic;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.GameModel;

namespace PoESkillTree.Computation.Data.GivenStats
{
    public class AllyLevelBasedStats : LevelBasedStats
    {
        public AllyLevelBasedStats(
            IBuilderFactories builderFactories, IModifierBuilder modifierBuilder, MonsterBaseStats monsterBaseStats)
            : base(builderFactories, modifierBuilder, monsterBaseStats)
        {
        }

        public override IReadOnlyList<Entity> AffectedEntities { get; } =
            new[] { GameModel.Entity.Totem, GameModel.Entity.Minion };

        protected override GivenStatCollection CreateCollection()
            => new GivenStatCollection(ModifierBuilder, ValueFactory)
            {
                { BaseSet, Life, LevelBased(l => MonsterBaseStats.AllyLife(l), "AllyLife") },
            };
    }
}