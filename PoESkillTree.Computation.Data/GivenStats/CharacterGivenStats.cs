using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;

namespace PoESkillTree.Computation.Data.GivenStats
{
    /// <summary>
    /// Given stats of player characters.
    /// </summary>
    /// <remarks>
    /// See http://pathofexile.gamepedia.com/Character and Metadata/Characters/Character.ot in GGPK.
    /// </remarks>
    public class CharacterGivenStats : UsesStatBuilders, IGivenStats
    {
        private readonly IModifierBuilder _modifierBuilder;
        private readonly Lazy<IReadOnlyList<IIntermediateModifier>> _lazyGivenStats;

        public CharacterGivenStats(IBuilderFactories builderFactories, IModifierBuilder modifierBuilder)
            : base(builderFactories)
        {
            _modifierBuilder = modifierBuilder;
            _lazyGivenStats = new Lazy<IReadOnlyList<IIntermediateModifier>>(() => CreateCollection().ToList());
        }

        public IReadOnlyList<Entity> AffectedEntities { get; } = new[] { Common.Entity.Character };

        public IReadOnlyList<string> GivenStatLines { get; } = new[]
        {
            // while Dual Wielding
            "10% more Attack Speed while Dual Wielding",
            "15% additional Block Chance while Dual Wielding",
            "20% more Attack Physical Damage while Dual Wielding",
            // charges
            "4% additional Physical Damage Reduction per Endurance Charge",
            "+4% to all Elemental Resistances per Endurance Charge",
            "4% increased Attack and Cast Speed per Frenzy Charge",
            "4% more Damage per Frenzy Charge",
            "40% increased Critical Strike Chance per Power Charge",
            // level based
            "+12 to maximum Life per Level",
            "+2 to Accuracy Rating per Level",
            "+3 to Evasion Rating per Level",
            "+6 to maximum Mana per Level",
            // attribute conversions
            "+1 to maximum Life per 2 Strength",
            "+1 to Strength Damage Bonus per Strength",
            "1% increased Melee Physical Damage per 5 Strength Damage Bonus ceiled",
            "+2 to Accuracy Rating per 1 Dexterity",
            "+1 to Dexterity Evasion Bonus per Dexterity",
            "1% increased Evasion Rating per 5 Dexterity Evasion Bonus ceiled",
            "+1 to Mana per 2 Intelligence ceiled",
            "1% increased maximum Energy Shield per 5 Intelligence ceiled",
            "-60% to all Elemental Resistances",
            "-60% to Chaos Resistance",
        };
        
        public IReadOnlyList<IIntermediateModifier> GivenModifiers => _lazyGivenStats.Value;

        private GivenStatCollection CreateCollection() => new GivenStatCollection(_modifierBuilder, ValueFactory)
        {
            // pools
            { BaseSet, Life, 38 },
            { BaseSet, Mana, 34 },
            { BaseSet, Mana.Regen.Percent, 1.75 },
            // other basic stats
            { BaseSet, Evasion, 53 },
            { BaseSet, Stat.Accuracy, -2 }, // 0 at level 1 with no dexterity
            // resistances
            { BaseSet, Physical.Resistance.Maximum, 90 },
            // traps, mines and totems
            { BaseSet, Traps.CombinedInstances.Maximum, 15 },
            { BaseSet, Mines.CombinedInstances.Maximum, 5 },
            { BaseSet, Totems.CombinedInstances.Maximum, 1 },
        };
    }
}