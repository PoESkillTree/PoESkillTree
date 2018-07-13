using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Forms;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;

namespace PoESkillTree.Computation.Data
{
    /// <inheritdoc />
    /// <summary>
    /// <see cref="IStatMatchers"/> implementation matching whole stat lines. Most of these would also work
    /// in <see cref="FormAndStatMatchers"/> but listing them here keeps special keystone/ascendancy mods in one place.
    /// </summary>
    public class SpecialMatchers : StatMatchersBase
    {
        private readonly IModifierBuilder _modifierBuilder;

        public SpecialMatchers(
            IBuilderFactories builderFactories, IMatchContexts matchContexts, IModifierBuilder modifierBuilder)
            : base(builderFactories, matchContexts)
        {
            _modifierBuilder = modifierBuilder;
        }

        public override bool MatchesWholeLineOnly => true;

        protected override IEnumerable<MatcherData> CreateCollection() =>
            new SpecialMatcherCollection(_modifierBuilder, ValueFactory)
            {
                {
                    @"\+# to level of socketed support gems",
                    BaseAdd, Value, Gem.IncreaseSupportLevel
                },
                {
                    "primordial",
                    BaseAdd, 1, Stat.PrimordialJewelsSocketed
                },
                {
                    "grand spectrum",
                    BaseAdd, 1, Stat.GrandSpectrumJewelsSocketed
                },
                {
                    "ignore all movement penalties from armour",
                    TotalOverride, 1, Flag.IgnoreMovementSpeedPenalties
                },
                {
                    "life leech recovers based on your chaos damage instead",
                    BaseAdd, 100, Life.Leech.Of(Chaos.Invert.Damage).ConvertTo(Life.Leech.Of(Chaos.Damage))
                },
                // Keystones
                {
                    // Point Blank
                    "projectile attack hits deal up to #% more damage to targets at the start of their movement, " +
                    "dealing less damage to targets as the projectile travels farther",
                    PercentMore,
                    // 0 to 10: Value; 10 to 35: Value to 0; 35 to 150: 0 to -Value
                    Value * ValueFactory.LinearScale(Projectile.TravelDistance, (0, 1), (10, 1), (35, 0), (150, -1)),
                    Damage.With(DamageSource.Attack), With(Keyword.Projectile)
                },
                {
                    // Elemental Equilibrium
                    @"enemies you hit with elemental damage temporarily get \+#% resistance to those elements " +
                    "and -#% resistance to other elements",
                    ElementalEquilibrium().ToArray()
                },
                {
                    // Necromantic Aegis
                    "all bonuses from an equipped shield apply to your minions instead of you",
                    TotalOverride, 1, Flag.ShieldModifiersApplyToMinionsInstead
                },
                {
                    // Perfect Agony
                    "modifiers to critical strike multiplier also apply to damage multiplier for " +
                    "ailments from critical strikes at #% of their value",
                    TotalOverride, Value,
                    CriticalStrike.Multiplier.WithSkills.ApplyModifiersToAilments()
                },
                {
                    // Vaal Pact
                    "maximum life leech rate is doubled",
                    PercentMore, 100, Life.Leech.RateLimit
                },
                {
                    // Ancestral Bond
                    "you can't deal damage with skills yourself",
                    TotalOverride, 0, Damage, Not(Or(With(Keyword.Totem), With(Keyword.Trap), With(Keyword.Mine)))
                },
                // Ascendancies
                // - Juggernaut
                {
                    "you cannot be slowed to below base speed",
                    TotalOverride, 1, Stat.AnimationSpeed.Minimum
                },
                {
                    "movement speed cannot be modified to below base value",
                    TotalOverride, 1, Stat.MovementSpeed.Minimum
                },
                {
                    "armour received from body armour is doubled",
                    PercentMore, 100, Armour, Condition.BaseValueComesFrom(ItemSlot.BodyArmour)
                },
                // - Chieftain
                {
                    "totems are immune to fire damage",
                    TotalOverride, 100, Fire.Resistance.For(Entity.Totem)
                },
                {
                    "totems have #% of your armour",
                    BaseAdd, Value.AsPercentage * Armour.Value, Armour.For(Entity.Totem)
                },
                // - Deadeye
                {
                    "far shot",
                    PercentMore,
                    30 * ValueFactory.LinearScale(Projectile.TravelDistance, (0, 0), (150, 1)),
                    Damage.With(DamageSource.Attack), With(Keyword.Projectile)
                },
                {
                    // Ascendant
                    "projectiles gain damage as they travel further, dealing up to #% increased damage with hits to targets",
                    PercentIncrease,
                    Value * ValueFactory.LinearScale(Projectile.TravelDistance, (0, 0), (150, 1)),
                    Damage.WithHits, With(Keyword.Projectile)
                },
                // - Elementalist
                {
                    "#% increased damage of each damage type for which you have a matching golem",
                    LiegeOfThePrimordialDamage().ToArray()
                },
                {
                    "your elemental golems are immune to elemental damage",
                    TotalOverride, 100, Elemental.Resistance.For(Entity.Minion), And(With(Keyword.Golem), WithElemental)
                },
                {
                    "every # seconds: " +
                    "gain chilling conflux for # seconds " +
                    "gain shocking conflux for # seconds " +
                    "gain igniting conflux for # seconds " +
                    "gain chilling, shocking and igniting conflux for # seconds",
                    ShaperOfDesolation()
                },
                {
                    "for each element you've been hit by damage of recently, " +
                    "#% increased damage of that element",
                    ParagonOfCalamityDamage().ToArray()
                },
                {
                    "for each element you've been hit by damage of recently, " +
                    "#% reduced damage taken of that element",
                    ParagonOfCalamityDamageTaken().ToArray()
                },
                // - Necromancer
                {
                    "your offering skills also affect you",
                    TotalOverride, 1,
                    Buffs(Self, Entity.Minion).With(Keyword.Offering).ApplyToEntity(Self)
                },
                // - Champion
                {
                    "your hits permanently intimidate enemies that are on full life",
                    TotalOverride, 1, Buff.Intimidate.On(Enemy),
                    Action.Unique("On Hit against a full life Enemy").On
                },
                // - Slayer
                { 
                    "your damaging hits always stun enemies that are on full life", 
                    TotalOverride, 100, Effect.Stun.ChanceOn(Enemy),
                    Action.Unique("On damaging Hit against a full life Enemy").On
                },
                // - Inquisitor
                {
                    "critical strikes ignore enemy monster elemental resistances",
                    TotalOverride, 1, Elemental.IgnoreResistance, CriticalStrike.On
                },
                {
                    "non-critical strikes penetrate #% of enemy elemental resistances",
                    BaseAdd, Value, Elemental.Penetration, Action.NonCriticalStrike.On
                },
                // - Hierophant
                {
                    "gain #% of maximum mana as extra maximum energy shield",
                    BaseAdd, Value, Mana.ConvertTo(EnergyShield)
                },
                // - Guardian
                {
                    "grants armour equal to #% of your reserved life to you and nearby allies",
                    BaseAdd,
                    Value.AsPercentage * Life.Value * Life.Reservation.Value, Buff.Buff(Armour, Self, Ally)
                },
                {
                    "grants maximum energy shield equal to #% of your reserved mana to you and nearby allies",
                    BaseAdd,
                    Value.AsPercentage * Mana.Value * Mana.Reservation.Value, Buff.Buff(EnergyShield, Self, Ally)
                },
                {
                    "warcries cost no mana",
                    TotalOverride, 0, Mana.Cost, With(Keyword.Warcry)
                },
                {
                    "using warcries is instant",
                    TotalOverride, double.PositiveInfinity, Stat.CastSpeed, With(Keyword.Warcry)
                },
                // - Assassin
                {
                    // Ascendant
                    "your critical strikes with attacks maim enemies",
                    TotalOverride, 1, Buff.Maim.On(Enemy),
                    And(With(Keyword.Attack), CriticalStrike.On)
                },
                // - Trickster
                {
                    "movement skills cost no mana",
                    TotalOverride, 0, Mana.Cost, With(Keyword.Movement)
                },
            };

        private IEnumerable<(IFormBuilder form, IValueBuilder value, IStatBuilder stat, IConditionBuilder condition)>
            ElementalEquilibrium()
        {
            foreach (var type in ElementalDamageTypes)
            {
                IConditionBuilder EnemyHitBy(IDamageTypeBuilder damageType) =>
                    Action.HitWith(damageType).InPastXSeconds(ValueFactory.Create(5));

                yield return (BaseAdd, Values[0], type.Resistance.For(Enemy), EnemyHitBy(type));
                var otherTypes = ElementalDamageTypes.Except(type);
                yield return (BaseSubtract, Values[1], type.Resistance.For(Enemy),
                    And(Not(EnemyHitBy(type)), otherTypes.Select(EnemyHitBy).ToArray()));
            }
        }

        private IEnumerable<(IFormBuilder form, IValueBuilder value, IStatBuilder stat, IConditionBuilder condition)>
            LiegeOfThePrimordialDamage()
        {
            foreach (var type in AllDamageTypes)
            {
                yield return (PercentIncrease, Value, type.Damage, Skills[Keyword.Golem, type].Any(s => s.HasInstance));
            }
        }

        private (IFormBuilder form, double value, IStatBuilder stat, IConditionBuilder condition)[]
            ShaperOfDesolation()
        {
            var stats = new[]
            {
                Buff.Temporary(Buff.Conflux.Chilling, ShaperOfDesolationStep.Chilling),
                Buff.Temporary(Buff.Conflux.Shocking, ShaperOfDesolationStep.Shocking),
                Buff.Temporary(Buff.Conflux.Igniting, ShaperOfDesolationStep.Igniting),
                Buff.Temporary(Buff.Conflux.Chilling, ShaperOfDesolationStep.All),
                Buff.Temporary(Buff.Conflux.Shocking, ShaperOfDesolationStep.All),
                Buff.Temporary(Buff.Conflux.Igniting, ShaperOfDesolationStep.All),
            };

            return (
                from stat in stats
                select (TotalOverride, 1.0, stat, Condition.True)
            ).ToArray();
        }

        public enum ShaperOfDesolationStep
        {
            None,
            Chilling,
            Shocking,
            Igniting,
            All
        }

        private IEnumerable<(IFormBuilder form, IValueBuilder value, IStatBuilder stat, IConditionBuilder condition)>
            ParagonOfCalamityDamage()
        {
            foreach (var type in ElementalDamageTypes)
            {
                yield return (PercentIncrease, Value, type.Damage, Action.HitWith(type).By(Enemy).Recently);
            }
        }

        private IEnumerable<(IFormBuilder form, IValueBuilder value, IStatBuilder stat, IConditionBuilder condition)>
            ParagonOfCalamityDamageTaken()
        {
            foreach (var type in ElementalDamageTypes)
            {
                yield return (PercentReduce, Value, type.Damage.Taken, Action.HitWith(type).By(Enemy).Recently);
            }
        }
    }
}