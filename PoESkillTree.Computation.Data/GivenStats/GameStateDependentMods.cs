using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common.Builders;
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
            { BaseAdd, Stat.PassivePoints.Maximum, 2, MetaStats.SelectedBandit.Value.Eq((int) Bandit.None) },
            { BaseAdd, Life.Regen.Percent, 1, MetaStats.SelectedBandit.Value.Eq((int) Bandit.Oak) },
            { BaseAdd, Physical.Resistance, 2, MetaStats.SelectedBandit.Value.Eq((int) Bandit.Oak) },
            { PercentIncrease, Physical.Damage, 20, MetaStats.SelectedBandit.Value.Eq((int) Bandit.Oak) },
            { PercentIncrease, Stat.CastRate, 6, MetaStats.SelectedBandit.Value.Eq((int) Bandit.Kraityn) },
            { BaseAdd, Stat.Dodge.AttackChance, 3, MetaStats.SelectedBandit.Value.Eq((int) Bandit.Kraityn) },
            { PercentIncrease, Stat.MovementSpeed, 6, MetaStats.SelectedBandit.Value.Eq((int) Bandit.Kraityn) },
            { BaseAdd, Mana.Regen, 5, MetaStats.SelectedBandit.Value.Eq((int) Bandit.Alira) },
            { BaseAdd, CriticalStrike.Multiplier.WithHits, 20, MetaStats.SelectedBandit.Value.Eq((int) Bandit.Alira) },
            { BaseAdd, Elemental.Resistance, 15, MetaStats.SelectedBandit.Value.Eq((int) Bandit.Alira) },
            // Resistance penalties
            { BaseSet, MetaStats.SelectedQuestPart, (int) QuestPart.Epilogue },
            { BaseSubtract, Elemental.Resistance, 30, MetaStats.SelectedQuestPart.Value.Eq((int) QuestPart.PartTwo) },
            { BaseSubtract, Chaos.Resistance, 30, MetaStats.SelectedQuestPart.Value.Eq((int) QuestPart.PartTwo) },
            { BaseSubtract, Elemental.Resistance, 60, MetaStats.SelectedQuestPart.Value.Eq((int) QuestPart.Epilogue) },
            { BaseSubtract, Chaos.Resistance, 60, MetaStats.SelectedQuestPart.Value.Eq((int) QuestPart.Epilogue) },
        };
    }
}