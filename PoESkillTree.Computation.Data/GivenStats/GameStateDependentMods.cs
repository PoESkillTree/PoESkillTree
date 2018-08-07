using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;

namespace PoESkillTree.Computation.Data.GivenStats
{
    /// <summary>
    /// Modifiers of the player character that depend on the game state, e.g. bandits or resistance penalties.
    /// </summary>
    public class GameStateDependentMods : UsesStatBuilders, IGivenStats
    {
        private readonly IModifierBuilder _modifierBuilder;
        private readonly IMetaStatBuilders _stat;
        private readonly Lazy<IReadOnlyList<IIntermediateModifier>> _lazyGivenStats;

        public GameStateDependentMods(
            IBuilderFactories builderFactories, IModifierBuilder modifierBuilder, IMetaStatBuilders metaStatBuilders)
            : base(builderFactories)
        {
            _modifierBuilder = modifierBuilder;
            _stat = metaStatBuilders;
            _lazyGivenStats = new Lazy<IReadOnlyList<IIntermediateModifier>>(() => CreateCollection().ToList());
        }

        public IReadOnlyList<Entity> AffectedEntities { get; } = new[] { Common.Entity.Character };
        public IReadOnlyList<string> GivenStatLines { get; } = new string[0];
        public IReadOnlyList<IIntermediateModifier> GivenModifiers => _lazyGivenStats.Value;

        private GivenStatCollection CreateCollection() => new GivenStatCollection(_modifierBuilder, ValueFactory)
        {
            // Bandits
            { BaseSet, _stat.SelectedBandit, (int) Bandit.None },
            { BaseAdd, Life.Regen.Percent, 1, _stat.SelectedBandit.Value.Eq((int) Bandit.Oak) },
            { BaseAdd, Physical.Resistance, 2, _stat.SelectedBandit.Value.Eq((int) Bandit.Oak) },
            { PercentIncrease, Physical.Damage, 20, _stat.SelectedBandit.Value.Eq((int) Bandit.Oak) },
            { PercentIncrease, Stat.CastRate, 6, _stat.SelectedBandit.Value.Eq((int) Bandit.Kraityn) },
            { BaseAdd, Stat.Dodge.AttackChance, 3, _stat.SelectedBandit.Value.Eq((int) Bandit.Kraityn) },
            { PercentIncrease, Stat.MovementSpeed, 6, _stat.SelectedBandit.Value.Eq((int) Bandit.Kraityn) },
            { BaseAdd, Mana.Regen, 5, _stat.SelectedBandit.Value.Eq((int) Bandit.Alira) },
            { BaseAdd, CriticalStrike.Multiplier.WithHits, 20, _stat.SelectedBandit.Value.Eq((int) Bandit.Alira) },
            { BaseAdd, Elemental.Resistance, 15, _stat.SelectedBandit.Value.Eq((int) Bandit.Alira) },
            // Resistance penalties
            { BaseSet, _stat.SelectedQuestPart, (int) QuestPart.Epilogue },
            { BaseSubtract, Elemental.Resistance, 30, _stat.SelectedQuestPart.Value.Eq((int) QuestPart.PartTwo) },
            { BaseSubtract, Chaos.Resistance, 30, _stat.SelectedQuestPart.Value.Eq((int) QuestPart.PartTwo) },
            { BaseSubtract, Elemental.Resistance, 60, _stat.SelectedQuestPart.Value.Eq((int) QuestPart.Epilogue) },
            { BaseSubtract, Chaos.Resistance, 60, _stat.SelectedQuestPart.Value.Eq((int) QuestPart.Epilogue) },
        };
    }
}