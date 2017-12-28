using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Parsing.Builders;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Damage;
using PoESkillTree.Computation.Parsing.Builders.Forms;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Builders.Values;
using PoESkillTree.Computation.Parsing.Data;
using PoESkillTree.Computation.Parsing.ModifierBuilding;

namespace PoESkillTree.Computation.Data
{
    public class SpecialMatchers : UsesMatchContext, IStatMatchers
    {
        private readonly IModifierBuilder _modifierBuilder;

        public SpecialMatchers(IBuilderFactories builderFactories, 
            IMatchContexts matchContexts, IModifierBuilder modifierBuilder) 
            : base(builderFactories, matchContexts)
        {
            _modifierBuilder = modifierBuilder;
        }

        public bool MatchesWholeLineOnly => true;

        public IEnumerator<MatcherData> GetEnumerator() => new SpecialMatcherCollection(
            _modifierBuilder, ValueFactory)
        {
            {
                @"\+# to level of socketed support gems",
                BaseAdd, Value, Gem.IncreaseLevel(onlySupportGems: true)
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
                "ignore all movement penalties from armour", TotalOverride, 1,
                Flag.IgnoreMovementSpeedPenalties
            },
            {
                "life leech is based on your chaos damage instead", TotalOverride, 1,
                Life.Leech.BasedOn(Chaos)
            },
            // Keystones
            {
                // Point Blank
                "projectile attack hits deal up to #% more damage to targets at the start of their movement, " +
                "dealing less damage to targets as the projectile travels farther",
                PercentMore,
                // 0 to 10: Value; 10 to 35: Value to 0; 35 to 150: 0 to -Value
                Value * ValueFactory.LinearScale(Projectile.TravelDistance,
                    (0, 1), (10, 1), (35, 0), (150, -1)),
                Damage, And(Damage.With(Source.Attack), With(Skills[Keyword.Projectile]), Hit.On())
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
                (TotalOverride, OffHand.AppliesToMinions, 1, OffHand.Has(Tags.Shield)),
                (TotalOverride, OffHand.AppliesToSelf, 0, OffHand.Has(Tags.Shield))
            },
            {
                // Perfect Agony
                "modifiers to critical strike multiplier also apply to damage multiplier for " +
                "ailments from critical strikes at #% of their value",
                TotalOverride,
                1, CriticalStrike.Multiplier.ApplyModifiersTo(CriticalStrike.AilmentMultiplier,
                    percentOfTheirValue: Value)
            },
            {
                // Vaal Pact
                "maximum life leech rate is doubled",
                PercentMore, 100, Life.Leech.RateLimit
            },
            {
                // Ancestral Bond
                "you can't deal damage with skills yourself",
                TotalOverride, 0, Damage,
                Not(Or(With(Totems), With(Traps), With(Mines), With(Minions)))
            },
            // Ascendancies
            {
                "your hits permanently intimidate enemies that are on full life",
                TotalOverride, 1, Buff.Intimidate.On(Enemy),
                Condition.Unique("on Hit against Enemies that are on Full Life")
            },
            { "movement skills cost no mana", TotalOverride, 0, Skills[Keyword.Movement].Cost },
            {
                "your offering skills also affect you",
                TotalOverride, 1,
                Combine(Skill.BoneOffering, Skill.FleshOffering, Skill.SpiritOffering)
                    .ApplyStatsToEntity(Self)
            },
            {
                "far shot",
                PercentMore,
                30 * ValueFactory.LinearScale(Projectile.TravelDistance, (0, 0), (150, 1)),
                Damage, And(Damage.With(Source.Attack), With(Skills[Keyword.Projectile]))
            },
            {
                "projectiles gain damage as they travel further, dealing up to #% increased damage with hits to targets",
                PercentIncrease,
                Value * ValueFactory.LinearScale(Projectile.TravelDistance, (0, 0), (150, 1)),
                Damage, And(With(Skills[Keyword.Projectile]), Hit.On())
            },
            {
                "your critical strikes with attacks maim enemies",
                TotalOverride, 1, Buff.Maim.On(Enemy),
                And(Damage.With(Source.Attack), CriticalStrike.Against(Enemy).On())
            },
            {
                "gain #% of maximum mana as extra maximum energy shield",
                BaseAdd, Value, Mana.ConvertTo(EnergyShield)
            },
            {
                "critical strikes ignore enemy monster elemental resistances",
                TotalOverride, 1, Elemental.IgnoreResistance, CriticalStrike.Against(Enemy).On()
            },
            {
                "non-critical strikes penetrate #% of enemy elemental resistances",
                BaseAdd, Value, Elemental.Penetration, Action.NonCriticalStrike.Against(Enemy).On()
            },
            {
                "totems are immune to fire damage",
                TotalOverride, 100, Fire.Resistance, For(Entity.Totem)
            },
            {
                "totems have #% of your armour",
                BaseAdd, Value.AsPercentage * Entity.ModififerSource.Stat(Armour).Value,
                Armour, For(Entity.Totem)
            },
            // Elementalist
            {
                "#% increased damage of each damage type for which you have a matching golem",
                LiegeOfThePrimordialDamage().ToArray()
            },
            {
                "your elemental golems are immune to elemental damage",
                TotalOverride, 100,
                Elemental.Resistance, For(Entity.Minion.With(Keyword.Golem, Elemental))
            },
            {
                "every # seconds: " +
                "gain chilling conflux for # seconds " +
                "gain shocking conflux for # seconds " +
                "gain igniting conflux for # seconds " +
                "gain chilling, shocking and igniting conflux for # seconds",
                TotalOverride, 1, Buff.Rotation(Values[0])
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
                BaseAdd,
                Value.AsPercentage * Life.Value * Life.Reservation.Value, Armour.AsAura(Self, Ally)
            },
            {
                "grants maximum energy shield equal to #% of your reserved mana to you and nearby allies",
                BaseAdd,
                Value.AsPercentage * Mana.Value * Mana.Reservation.Value, EnergyShield.AsAura(Self, Ally)
            },
            { "warcries cost no mana", TotalOverride, 0, Skills[Keyword.Warcry].Cost },
            {
                "using warcries is instant",
                TotalOverride, double.PositiveInfinity, Skills[Keyword.Warcry].Speed
            },
            // Juggernaut
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
                PercentMore, 100,
                Armour, Condition.BaseValueComesFrom(Equipment[ItemSlot.BodyArmour])
            },
        }.GetEnumerator();

        private IEnumerable<(IFormBuilder form, IStatBuilder stat, IValueBuilder value, IConditionBuilder condition)> 
            ElementalEquilibrium()
        {
            foreach (var type in ElementalDamageTypes)
            {
                IConditionBuilder EnemyHitBy(IDamageTypeBuilder damageType) =>
                    Hit.With(damageType).Against(Enemy).InPastXSeconds(ValueFactory.Create(5));

                yield return (BaseAdd, Enemy.Stat(type.Resistance), Values[0],
                    EnemyHitBy(type));
                yield return (BaseSubtract, Enemy.Stat(type.Resistance), Values[1],
                    And(Not(EnemyHitBy(type)), EnemyHitBy(Elemental.Except(type))));
            }
        }

        private IEnumerable<(IFormBuilder form, IStatBuilder stat, IValueBuilder value, IConditionBuilder condition)> 
            LiegeOfThePrimordialDamage()
        {
            foreach (var type in AllDamageTypes)
            {
                yield return (PercentIncrease, type.Damage, Value, Golems[type].Any(s => s.HasInstance));
            }
        }

        private IEnumerable<(IFormBuilder form, IStatBuilder stat, IValueBuilder value, IConditionBuilder condition)>
            ParagonOfCalamity(IFormBuilder form, IStatBuilder stat, IValueBuilder value)
        {
            foreach (var type in ElementalDamageTypes)
            {
                yield return (form, stat, value, And(type.Damage.With(), Hit.With(type).Taken.Recently));
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}