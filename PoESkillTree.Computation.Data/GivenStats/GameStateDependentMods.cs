using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.GameModel;

namespace PoESkillTree.Computation.Data.GivenStats
{
    /// <summary>
    /// Modifiers of the player character that depend on the game state, e.g. bandits or resistance penalties.
    /// </summary>
    public class GameStateDependentMods : UsesStatBuilders, IGivenStats
    {
        private readonly IModifierBuilder _modifierBuilder;
        private readonly Lazy<IReadOnlyList<IIntermediateModifier>> _lazyGivenStats;

        public GameStateDependentMods(IBuilderFactories builderFactories, IModifierBuilder modifierBuilder)
            : base(builderFactories)
        {
            _modifierBuilder = modifierBuilder;
            _lazyGivenStats = new Lazy<IReadOnlyList<IIntermediateModifier>>(() => CreateCollection().ToList());
        }

        private IMetaStatBuilders MetaStats => BuilderFactories.MetaStatBuilders;

        public IReadOnlyList<Entity> AffectedEntities { get; } = new[] { GameModel.Entity.Character };
        public IReadOnlyList<string> GivenStatLines { get; } = new string[0];
        public IReadOnlyList<IIntermediateModifier> GivenModifiers => _lazyGivenStats.Value;

        private GivenStatCollection CreateCollection() => new GivenStatCollection(_modifierBuilder, ValueFactory)
        {
            // Bandits
            { BaseSet, MetaStats.SelectedBandit, (int) Bandit.None },
            { BaseAdd, Stat.PassivePoints.Maximum, 2, BanditIs(Bandit.None) },
            { BaseAdd, Life.Regen.Percent, 1, BanditIs(Bandit.Oak) },
            { BaseAdd, Physical.Resistance, 2, BanditIs(Bandit.Oak) },
            { PercentIncrease, Physical.Damage, 20, BanditIs(Bandit.Oak) },
            { PercentIncrease, Stat.CastRate, 6, BanditIs(Bandit.Kraityn) },
            { BaseAdd, Stat.Dodge.AttackChance, 3, BanditIs(Bandit.Kraityn) },
            { PercentIncrease, Stat.MovementSpeed, 6, BanditIs(Bandit.Kraityn) },
            { BaseAdd, Mana.Regen, 5, BanditIs(Bandit.Alira) },
            { BaseAdd, CriticalStrike.Multiplier.WithHits, 20, BanditIs(Bandit.Alira) },
            { BaseAdd, Elemental.Resistance, 15, BanditIs(Bandit.Alira) },
            // Resistance penalties
            { BaseSubtract, Elemental.Resistance, 30, QuestPartIs(QuestPart.PartTwo) },
            { BaseSubtract, Chaos.Resistance, 30, QuestPartIs(QuestPart.PartTwo) },
            { BaseSubtract, Elemental.Resistance, 60, QuestPartIs(QuestPart.Epilogue) },
            { BaseSubtract, Chaos.Resistance, 60, QuestPartIs(QuestPart.Epilogue) },
            // Boss type
            { PercentLess, Buffs(targets: Enemy).With(Keyword.Curse).Effect, 60, BossTypeIs(BossType.StandardBoss) },
            { BaseAdd, Elemental.Resistance.For(Enemy), 30, BossTypeIs(BossType.StandardBoss) },
            { BaseAdd, Chaos.Resistance.For(Enemy), 15, BossTypeIs(BossType.StandardBoss) },
            { PercentLess, Buffs(targets: Enemy).With(Keyword.Curse).Effect, 80, BossTypeIs(BossType.Shaper) },
            { BaseAdd, Elemental.Resistance.For(Enemy), 40, BossTypeIs(BossType.Shaper) },
            { BaseAdd, Chaos.Resistance.For(Enemy), 25, BossTypeIs(BossType.Shaper) },
        };

        private IConditionBuilder BanditIs(Bandit bandit)
            => MetaStats.SelectedBandit.Value.Eq((int) bandit);

        private IConditionBuilder QuestPartIs(QuestPart questPart)
            => MetaStats.SelectedQuestPart.Value.Eq((int) questPart);

        private IConditionBuilder BossTypeIs(BossType bossType)
            => MetaStats.SelectedBossType.Value.Eq((int) bossType);
    }
}