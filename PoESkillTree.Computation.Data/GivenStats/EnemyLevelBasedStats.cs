using System.Collections.Generic;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.GameModel;

namespace PoESkillTree.Computation.Data.GivenStats
{
    public class EnemyLevelBasedStats : LevelBasedStats
    {
        public EnemyLevelBasedStats(
            IBuilderFactories builderFactories, IModifierBuilder modifierBuilder, MonsterBaseStats monsterBaseStats)
            : base(builderFactories, modifierBuilder, monsterBaseStats)
        {
        }

        public override IReadOnlyList<Entity> AffectedEntities { get; } = new[] { GameModel.Entity.Enemy };

        protected override GivenStatCollection CreateCollection()
            => new GivenStatCollection(ModifierBuilder, ValueFactory)
            {
                { BaseSet, Stat.Level, ValueFactory.Minimum(Stat.Level.For(Entity.Character).Value, 84) },
                { BaseSet, Life, LevelBased(l => MonsterBaseStats.EnemyLife(l), "EnemyLife") },
                { BaseSet, Stat.Accuracy, LevelBased(l => MonsterBaseStats.Accuracy(l), "Accuracy") },
                { BaseSet, Stat.Evasion, LevelBased(l => MonsterBaseStats.Evasion(l), "Evasion") },
                {
                    BaseSet, Physical.Damage.WithSkills(DamageSource.Attack),
                    LevelBased(l => MonsterBaseStats.PhysicalDamage(l), "PhysicalDamage") * 1.5
                },
            };
    }
}