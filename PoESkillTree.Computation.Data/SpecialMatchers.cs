using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Providers;
using PoESkillTree.Computation.Providers.Conditions;
using PoESkillTree.Computation.Providers.Forms;
using PoESkillTree.Computation.Providers.Matching;
using PoESkillTree.Computation.Providers.Stats;
using PoESkillTree.Computation.Providers.Values;

namespace PoESkillTree.Computation.Data
{
    public class SpecialMatchers : UsesMatchContext, IStatMatchers
    {
        public SpecialMatchers(IProviderFactories providerFactories, 
            IMatchContextFactory matchContextFactory) 
            : base(providerFactories, matchContextFactory)
        {
            StatMatchers = CreateCollection();
        }

        public IEnumerable<object> StatMatchers { get; }

        private SpecialMatcherCollection CreateCollection() => new SpecialMatcherCollection
        {
            {
                @"\+# to level of socketed support gems",
                BaseAdd, Gem.IncreaseLevel(onlySupportGems: true)
            },
            {
                "primordial",
                BaseAdd, Stat.PrimordialJewelsSocketed, 1
            },
            {
                "grand spectrum",
                BaseAdd, Stat.GrandSpectrumJewelsSocketed, 1
            },
            {
                "ignore all movement penalties from armour", SetFlag,
                Flag.IgnoreMovementSpeedPenalties
            },
            {
                "life leech is based on your chaos damage instead", SetFlag,
                Life.Leech.BasedOn(Chaos)
            },
            // Keystones
            {
                // Point Blank
                "projectile attacks deal up to #% more damage to targets at the start of their movement, " +
                "dealing less damage to targets as the projectile travels farther",
                PercentMore, Damage, Value,
                // 0 to 10: Value; 10 to 35: Value to 0; 35 to 150: 0 to -Value
                ValueFactory.LinearScale(Projectile.TravelDistance,
                    (0, 1), (10, 1), (35, 0), (150, -1)),
                And(Damage.With(Source.Attack), With(Skills[Keyword.Projectile]))
            },
            {
                // Elemental Equilibrium
                "enemies you hit with elemental damage temporarily get +#% resistance to those elements " +
                "and -#% resistance to other elements",
                ElementalEquilibrium().ToArray()
            },
            {
                // Necromantic Aegis
                "all bonuses from an equipped shield apply to your minions instead of you",
                (TotalOverride, OffHand.AppliesToMinions, 1, OffHand.Has(Tags.Shield)),
                (TotalOverride, OffHand.AppliesToSelf, 0, OffHand.Has(Tags.Shield))
            },
            {
                // Perfect Agony
                "modifiers to critical strike multiplier also apply to damage multiplier for " +
                "ailments from critical strikes at #% of their value",
                TotalOverride,
                CriticalStrike.Multiplier.ApplyModifiersTo(CriticalStrike.AilmentMultiplier,
                    percentOfTheirValue: Value),
                1
            },
            // Ascendancies
            {
                "your hits permanently intimidate enemies that are on full life",
                SetFlag, Buff.Intimidate.On(Enemy),
                Condition.Unique("on Hit against Enemies that are on Full Life")
            },
            { "movement skills cost no mana", Zero, Skills[Keyword.Movement].Cost },
            {
                "your offering skills also affect you",
                SetFlag, Combine(Skill.BoneOffering, Skill.FleshOffering, Skill.SpiritOffering)
                    .ApplyStatsToEntity(Self)
            },
            {
                "far shot",
                PercentMore, Damage, 30,
                ValueFactory.LinearScale(Projectile.TravelDistance, (0, 0), (150, 1)),
                And(Damage.With(Source.Attack), With(Skills[Keyword.Projectile]))
            },
            {
                "projectiles gain damage as they travel further, dealing up to #% increased damage to targets",
                PercentIncrease, Damage, Value,
                ValueFactory.LinearScale(Projectile.TravelDistance, (0, 0), (150, 1)),
                With(Skills[Keyword.Projectile])
            },
            {
                "your critical strikes with attacks maim enemies",
                SetFlag, Buff.Maim.On(Enemy),
                And(Damage.With(Source.Attack), CriticalStrike.Against(Enemy).On())
            },
            {
                "gain #% of maximum mana as extra maximum energy shield",
                BaseAdd, Mana.ConvertTo(EnergyShield), Value
            },
            {
                "critical strikes ignore enemy monster elemental resistance",
                SetFlag, Elemental.IgnoreResistance, CriticalStrike.Against(Enemy).On()
            },
            {
                "non-critical strikes penetrate #% of enemy elemental resistances",
                BaseAdd, Elemental.Penetration, Action.NonCriticalStrike.Against(Enemy).On()
            },
            {
                "totems are immune to fire damage",
                TotalOverride, Fire.Resistance, 100, For(Entity.Totem)
            },
            {
                "totems have #% of your armour",
                BaseAdd, Armour, Value.AsPercentage * Entity.Character.Stat(Armour).Value,
                For(Entity.Totem)
            },
            // Elementalist
            {
                "#% increased damage of each damage type for which you have a matching golem",
                LiegeOfThePrimordialDamage().ToArray()
            },
            {
                "your elemental golems are immune to elemental damage",
                TotalOverride, Elemental.Resistance, 100,
                For(Entity.Minion.With(Keyword.Golem, Elemental))
            },
            {
                "every # seconds: " +
                "gain chilling conflux for # seconds " +
                "gain shocking conflux for # seconds " +
                "gain igniting conflux for # seconds " +
                "gain chilling, shocking and igniting conflux for # seconds",
                SetFlag, Buff.Rotation(Values[0])
                    .Step(Values[1], Buff.Conflux.Chilling)
                    .Step(Values[2], Buff.Conflux.Shocking)
                    .Step(Values[3], Buff.Conflux.Igniting)
                    .Step(Values[4], Buff.Conflux.Chilling, Buff.Conflux.Igniting,
                        Buff.Conflux.Shocking)
            },
            {
                "for each element you've been hit by damage of recently, " +
                "#% increased damage of that element",
                ParagonOfCalamity(PercentIncrease, Damage, Value).ToArray()
            },
            {
                "for each element you've been hit by damage of recently, " +
                "#% reduced damage taken of that element",
                ParagonOfCalamity(PercentReduce, Damage.Taken, Value).ToArray()
            },
            // Guardian
            {
                "grants armour equal to #% of your reserved life to you and nearby allies",
                BaseAdd, Armour.AsAura, Value.AsPercentage * Life.Value * Life.Reservation.Value
            },
            {
                "grants maximum energy shield equal to #% of your reserved mana to you and nearby allies",
                BaseAdd, EnergyShield.AsAura,
                Value.AsPercentage * Mana.Value * Mana.Reservation.Value
            },
            { "warcries cost no mana", Zero, Skills[Keyword.Warcry].Cost },
            {
                "using warcries is instant",
                TotalOverride, Skills[Keyword.Warcry].Speed, double.PositiveInfinity
            },
            // Juggernaut
            {
                "you cannot be slowed to below base speed",
                TotalOverride, Stat.AnimationSpeed.Minimum, 1
            },
            {
                "movement speed cannot be modified to below base value",
                TotalOverride, Stat.MovementSpeed.Minimum, 1
            },
            {
                "armour received from body armour is doubled",
                PercentMore, Armour, 100,
                Condition.BaseValueComesFrom(Equipment[ItemSlot.BodyArmour])
            },
        };

        private IEnumerable<(IFormProvider form, IStatProvider stat, ValueProvider value,
            IConditionProvider condition)> ElementalEquilibrium()
        {
            foreach (var type in ElementalDamageTypes)
            {
                yield return (BaseAdd, Enemy.Stat(type.Resistance), Values[0],
                    Enemy.HitByInPastXSeconds(type, 5));
                yield return (BaseSubtract, Enemy.Stat(type.Resistance), Values[1],
                    And(Not(Enemy.HitByInPastXSeconds(type, 5)),
                        Enemy.HitByInPastXSeconds(Elemental.Except(type), 5)));
            }
        }

        private IEnumerable<(IFormProvider form, IStatProvider stat, ValueProvider value,
            IConditionProvider condition)> LiegeOfThePrimordialDamage()
        {
            foreach (var type in AllDamageTypes)
            {
                yield return (PercentIncrease, type.Damage, Value,
                    Golems[type].Any(s => s.HasInstance));
            }
        }

        private IEnumerable<(IFormProvider form, IStatProvider stat, ValueProvider value,
            IConditionProvider condition)> ParagonOfCalamity(IFormProvider form,
            IStatProvider stat, ValueProvider value)
        {
            foreach (var type in ElementalDamageTypes)
            {
                yield return (form, stat, value,
                    And(type.Damage.With(), Self.HitByRecently(type)));
            }
        }
    }
}