using System.Collections.Generic;
using System.Linq;
using EnumsNET;
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
                    Damage.WithSkills(DamageSource.Attack), With(Keyword.Projectile)
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
                {
                    // Blood Magic
                    "spend life instead of mana for skills",
                    (BaseAdd, 100, Mana.Cost.ConvertTo(Life.Cost), Condition.True),
                    (TotalOverride, (int) Pool.Life, AllSkills.ReservationPool, Condition.True)
                },
                {
                    // Eldritch Battery: Display both mana and energy shield costs
                    "spend energy shield before mana for skill costs",
                    BaseAdd, 100, Mana.Cost.GainAs(EnergyShield.Cost)
                },
                // - Crimson Dance
                {
                    "your bleeding does not deal extra damage while the enemy is moving",
                    PercentLess, 50, Damage.With(Ailment.Bleed), Enemy.IsMoving
                },
                {
                    "you can inflict bleeding on an enemy up to 8 times",
                    BaseAdd, 7, Ailment.Bleed.InstancesOn(Self).Maximum
                },
                // Ascendancies
                // - Juggernaut
                {
                    "you cannot be slowed to below base speed",
                    TotalOverride, 1, Stat.ActionSpeed.Minimum
                },
                {
                    "movement speed cannot be modified to below base value",
                    TotalOverride, 1, Stat.MovementSpeed.Minimum
                },
                {
                    "armour received from body armour is doubled",
                    PercentMore, 100, Armour, Condition.BaseValueComesFrom(ItemSlot.BodyArmour)
                },
                { "gain accuracy rating equal to your strength", BaseAdd, Attribute.Strength.Value, Stat.Accuracy },
                { "#% increased attack speed per # accuracy rating", UndeniableAttackSpeed().ToArray() },
                {
                    "gain an endurance charge every second if you've been hit recently",
                    TotalOverride, 100, Charge.Endurance.ChanceToGain,
                    Action.Unique("Every second if you've been Hit recently").On
                },
                // - Berserker
                {
                    "recover #% of life and mana when you use a warcry",
                    (BaseAdd, Value.PercentOf(Life), Life.Gain, Skills[Keyword.Warcry].Cast.On),
                    (BaseAdd, Value.PercentOf(Mana), Mana.Gain, Skills[Keyword.Warcry].Cast.On)
                },
                {
                    "effects granted for having rage are doubled",
                    PercentMore, 100, Charge.RageEffect
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
                    Damage.WithSkills(DamageSource.Attack), With(Keyword.Projectile)
                },
                {
                    // Ascendant
                    "projectiles gain damage as they travel further, dealing up to #% increased damage with hits to targets",
                    PercentIncrease,
                    Value * ValueFactory.LinearScale(Projectile.TravelDistance, (0, 0), (150, 1)),
                    Damage.WithHits, With(Keyword.Projectile)
                },
                { "accuracy rating is doubled", PercentMore, 100, Stat.Accuracy },
                {
                    "if you've used a skill recently, you and nearby allies have tailwind",
                    TotalOverride, 1, Buff.Tailwind.On(Self).CombineWith(Buff.Tailwind.On(Ally)),
                    AllSkills.Cast.Recently
                },
                // - Occultist
                { "your curses can apply to hexproof enemies", TotalOverride, 1, Flag.IgnoreHexproof },
                {
                    "enemies you curse have malediction",
                    (PercentReduce, 10, Buff.Buff(Damage, Enemy), Buffs(Self, Enemy).With(Keyword.Curse).Any()),
                    (PercentIncrease, 10, Buff.Buff(Damage.Taken, Enemy), Buffs(Self, Enemy).With(Keyword.Curse).Any())
                },
                // - Elementalist
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
                { "cannot take reflected elemental damage", PercentLess, 100, Elemental.ReflectedDamageTaken },
                {
                    "gain #% increased area of effect for # seconds",
                    PercentIncrease, Values[0],
                    Buff.Temporary(Stat.AreaOfEffect, PendulumOfDestructionStep.AreaOfEffect)
                },
                {
                    "gain #% increased elemental damage for # seconds",
                    PercentIncrease, Values[0],
                    Buff.Temporary(Elemental.Damage, PendulumOfDestructionStep.ElementalDamage)
                },
                // - Necromancer
                {
                    "your offering skills also affect you",
                    TotalOverride, 1, Buffs(Self, Entity.Minion).With(Keyword.Offering).ApplyToEntity(Self)
                },
                {
                    "your offerings have #% reduced effect on you",
                    PercentLess, 50, Buffs(Self, Self).With(Keyword.Offering).Effect
                },
                {
                    "summoned skeletons' hits can't be evaded",
                    TotalOverride, 100, Stat.ChanceToHit.For(Entity.Minion), With(Skills.SummonSkeleton)
                },
                // - Gladiator
                {
                    "attacks maim on hit against bleeding enemies",
                    TotalOverride, 100, Buff.Maim.Chance, With(Keyword.Attack).And(Ailment.Bleed.IsOn(Enemy))
                },
                {
                    "your counterattacks deal double damage",
                    TotalOverride, 100, Damage.ChanceToDouble, With(Keyword.CounterAttack)
                },
                // - Champion
                {
                    "your hits permanently intimidate enemies that are on full life",
                    TotalOverride, 1, Buff.Intimidate.On(Enemy),
                    Action.Unique("On Hit against a full life Enemy").On
                },
                {
                    "enemies taunted by you cannot evade attacks",
                    TotalOverride, 0, Evasion.For(Enemy), Buff.Taunt.IsOn(Self, Enemy)
                },
                {
                    "gain adrenaline for # seconds when you reach low life if you do not have adrenaline",
                    (PercentIncrease, 100, Buff.Buff(Damage, Self), Condition.Unique("Do you have Adrenaline?")),
                    (PercentIncrease, 25, Buff.Buff(Stat.CastRate, Self), Condition.Unique("Do you have Adrenaline?")),
                    (PercentIncrease, 25, Buff.Buff(Stat.MovementSpeed, Self),
                        Condition.Unique("Do you have Adrenaline?")),
                    (BaseAdd, 10, Buff.Buff(Physical.Resistance, Self), Condition.Unique("Do you have Adrenaline?"))
                },
                // - Slayer
                {
                    "your damaging hits always stun enemies that are on full life",
                    TotalOverride, 100, Effect.Stun.Chance,
                    Action.Unique("On damaging Hit against a full life Enemy").On
                },
                { "cannot take reflected physical damage", PercentLess, 100, Physical.ReflectedDamageTaken },
                // - Inquisitor
                {
                    "critical strikes ignore enemy monster elemental resistances",
                    TotalOverride, 1, Elemental.IgnoreResistanceWithCrits
                },
                {
                    "non-critical strikes penetrate #% of enemy elemental resistances",
                    BaseAdd, Value, Elemental.PenetrationWithNonCrits
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
                    TotalOverride, double.PositiveInfinity, Stat.CastRate, With(Keyword.Warcry)
                },
                {
                    "#% additional block chance for # seconds every # seconds",
                    BaseAdd, Values[0], Block.AttackChance,
                    Condition.Unique("Is the additional Block Chance from Bastion of Hope active?")
                },
                // - Assassin
                {
                    // Ascendant
                    "your critical strikes with attacks maim enemies",
                    TotalOverride, 1, Buff.Maim.On(Enemy),
                    And(With(Keyword.Attack), CriticalStrike.On)
                },
                // - Trickster
                { "movement skills cost no mana", TotalOverride, 0, Mana.Cost, With(Keyword.Movement) },
                {
                    "your hits have #% chance to gain #% of non-chaos damage as extra chaos damage",
                    BaseAdd, Values[0] * Values[1] / 100, Chaos.Invert.Damage.WithHits.GainAs(Chaos.Damage.WithHits)
                },
                // - Saboteur
                { "nearby enemies are blinded", TotalOverride, 1, Buff.Blind.On(Enemy), Enemy.IsNearby },
            };

        private IEnumerable<(IFormBuilder form, IValueBuilder value, IStatBuilder stat, IConditionBuilder condition)>
            UndeniableAttackSpeed()
        {
            var attackSpeed = Stat.CastRate.With(DamageSource.Attack);
            foreach (var hand in Enums.GetValues<AttackDamageHand>())
            {
                IValueBuilder PerAccuracy(ValueBuilder value) =>
                    ValueBuilderUtils.PerStat(Stat.Accuracy.With(hand), Values[1])(value);

                yield return (PercentIncrease, PerAccuracy(Values[0]), attackSpeed.With(hand), Condition.True);
            }
        }

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

        public enum PendulumOfDestructionStep
        {
            None,
            AreaOfEffect,
            ElementalDamage
        }
    }
}