using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.GameModel;

namespace PoESkillTree.Computation.Data.GivenStats
{
    /// <summary>
    /// Given stats of player characters.
    /// </summary>
    /// <remarks>
    /// See http://pathofexile.gamepedia.com/Character and Metadata/Characters/Character.ot in GGPK.
    /// </remarks>
    public class CharacterGivenStats : UsesConditionBuilders, IGivenStats
    {
        private readonly IModifierBuilder _modifierBuilder;
        private readonly Lazy<IReadOnlyList<IIntermediateModifier>> _lazyGivenStats;
        private readonly CharacterBaseStats _characterBaseStats;

        public CharacterGivenStats(
            IBuilderFactories builderFactories, IModifierBuilder modifierBuilder, CharacterBaseStats characterBaseStats)
            : base(builderFactories)
        {
            _modifierBuilder = modifierBuilder;
            _characterBaseStats = characterBaseStats;
            _lazyGivenStats = new Lazy<IReadOnlyList<IIntermediateModifier>>(() => CreateCollection().ToList());
        }

        public IReadOnlyList<Entity> AffectedEntities { get; } = new[] { GameModel.Entity.Character };

        public IReadOnlyList<string> GivenStatLines { get; } = new[]
        {
            // while Dual Wielding
            "10% more Attack Speed while Dual Wielding",
            "+15% chance to block attack damage while Dual Wielding",
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
            // Rampage
            "Minions deal 2% increased Damage per 10 Rampage Stacks",
            "Minions gain 1% increased Movement Speed per 10 Rampage Stacks",
        };

        public IReadOnlyList<IIntermediateModifier> GivenModifiers => _lazyGivenStats.Value;

        private GivenStatCollection CreateCollection() => new GivenStatCollection(_modifierBuilder, ValueFactory)
        {
            // passive points
            { BaseSet, Stat.PassivePoints.Maximum, Stat.Level.Value - 1 },
            { BaseAdd, Stat.PassivePoints.Maximum, 22 },
            { BaseSet, Stat.AscendancyPassivePoints.Maximum, 8 },
            // pools
            { BaseSet, Life, CharacterClassBased(_characterBaseStats.Life, "Life") },
            { BaseSet, Mana, CharacterClassBased(_characterBaseStats.Mana, "Mana") },
            { BaseSet, Mana.Regen.Percent, 1.75 },
            // other basic stats
            { BaseSet, Attribute.Strength, CharacterClassBased(_characterBaseStats.Strength, "Strength") },
            { BaseSet, Attribute.Dexterity, CharacterClassBased(_characterBaseStats.Dexterity, "Dexterity") },
            { BaseSet, Attribute.Intelligence, CharacterClassBased(_characterBaseStats.Intelligence, "Intelligence") },
            { BaseSet, Evasion, 53 },
            { BaseSet, Stat.Accuracy, -2 }, // 0 at level 1 with no dexterity
            { BaseSet, CriticalStrike.Multiplier, 150 },
            // resistances
            { BaseSet, Physical.Resistance.Maximum, 90 },
            // traps, mines and totems
            { BaseSet, Traps.CombinedInstances.Maximum, 15 },
            { BaseSet, Mines.CombinedInstances.Maximum, 5 },
            { BaseSet, Totems.CombinedInstances.Maximum, 1 },
            // rage
            { BaseSet, Charge.Rage.Amount.Maximum, 50 },
            { BaseSet, Charge.RageEffect, 1 },
            {
                PercentIncrease, Damage.WithSkills(DamageSource.Attack),
                Charge.Rage.Amount.Value * Charge.RageEffect.Value
            },
            {
                PercentIncrease, Stat.CastRate.With(DamageSource.Attack),
                PerStat(Charge.Rage.Amount, 2) * Charge.RageEffect.Value
            },
            { PercentIncrease, Stat.MovementSpeed, PerStat(Charge.Rage.Amount, 5) * Charge.RageEffect.Value },
            { BaseSubtract, Life.Regen.Percent, 0.1 * Charge.Rage.Amount.Value * Charge.RageEffect.Value },
            // unarmed
            {
                BaseSet, Stat.Range,
                CharacterClassBased(_characterBaseStats.UnarmedRange, "UnarmedRange"), Not(MainHand.HasItem)
            },
            { BaseSet, CriticalStrike.Chance.With(AttackDamageHand.MainHand), 0, Not(MainHand.HasItem) },
            {
                BaseSet, Stat.BaseCastTime.With(AttackDamageHand.MainHand),
                CharacterClassBased(_characterBaseStats.UnarmedAttackTime, "UnarmedAttackTime") / 1000,
                Not(MainHand.HasItem)
            },
            {
                BaseSet, Physical.Damage.WithSkills.With(AttackDamageHand.MainHand),
                Stat.CharacterClass.Value.Select(
                    c => UnarmedPhysicalDamage((CharacterClass) (int) c.Single),
                    c => $"{c}.UnarmedPhysicalDamage"),
                Not(MainHand.HasItem)
            },
            // configuration
            {
                TotalOverride, Charge.Endurance.Amount, Charge.Endurance.Amount.Maximum.Value,
                Condition.Unique("Endurance.Charge.Amount.SetToMaximum")
            },
            {
                TotalOverride, Charge.Power.Amount, Charge.Power.Amount.Maximum.Value,
                Condition.Unique("Power.Charge.Amount.SetToMaximum")
            },
            {
                TotalOverride, Charge.Frenzy.Amount, Charge.Frenzy.Amount.Maximum.Value,
                Condition.Unique("Frenzy.Charge.Amount.SetToMaximum")
            },
            // configuration
            { TotalOverride, Buff.Onslaught.On(Self), 1, Condition.Unique("Onslaught.ExplicitlyActive") },
            { TotalOverride, Buff.UnholyMight.On(Self), 1, Condition.Unique("UnholyMight.ExplicitlyActive") },
            { TotalOverride, Buff.Fortify.On(Self), 1, Condition.Unique("Fortify.ExplicitlyActive") },
            { TotalOverride, Buff.Tailwind.On(Self), 1, Condition.Unique("Tailwind.ExplicitlyActive") },
            { TotalOverride, Buff.Maim.On(Enemy), 1, Condition.Unique("Maim.ExplicitlyActiveOnEnemy") },
            { TotalOverride, Buff.Blind.On(Enemy), 1, Condition.Unique("Blind.ExplicitlyActiveOnEnemy") },
            { TotalOverride, Buff.Intimidate.On(Enemy), 1, Condition.Unique("Intimidate.ExplicitlyActiveOnEnemy") },
            { TotalOverride, Buff.CoveredInAsh.On(Enemy), 1, Condition.Unique("CoveredInAsh.ExplicitlyActiveOnEnemy") },
        };

        private ValueBuilder CharacterClassBased(Func<CharacterClass, int> selector, string identity)
            => Stat.CharacterClass.Value.Select(v => selector((CharacterClass) (int) v), v => $"{v}.{identity}");

        private static ValueBuilder PerStat(IStatBuilder stat, double divideBy)
            => (stat.Value / divideBy).Select(Math.Floor, o => $"Floor({o})");

        private NodeValue UnarmedPhysicalDamage(CharacterClass c)
            => new NodeValue(_characterBaseStats.UnarmedPhysicalDamageMinimum(c),
                _characterBaseStats.UnarmedPhysicalDamageMaximum(c));
    }
}