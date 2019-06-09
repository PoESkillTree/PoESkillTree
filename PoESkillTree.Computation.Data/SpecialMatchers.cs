using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Forms;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.PassiveTree;
using PoESkillTree.Utils.Extensions;

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

        public SpecialMatchers(IBuilderFactories builderFactories, IModifierBuilder modifierBuilder)
            : base(builderFactories)
        {
            _modifierBuilder = modifierBuilder;
        }

        public override bool MatchesWholeLineOnly => true;

        protected override IReadOnlyList<MatcherData> CreateCollection() =>
            new FormAndStatMatcherCollection(_modifierBuilder, ValueFactory)
            {
                {
                    @"\+# to level of socketed support gems",
                    BaseAdd, Value, Gem.IncreaseSupportLevel
                },
                {
                    "ignore all movement penalties from armour",
                    TotalOverride, 0,
                    Stat.MovementSpeed.AsItemPropertyForSlot(ItemSlot.BodyArmour),
                    Stat.MovementSpeed.AsItemPropertyForSlot(ItemSlot.OffHand)
                },
                {
                    "life leech recovers based on your chaos damage instead",
                    BaseAdd, 100, Life.Leech.Of(Chaos.Invert.Damage).ConvertTo(Life.Leech.Of(Chaos.Damage))
                },
                {
                    "strength's damage bonus instead grants 3% increased melee physical damage per 10 strength",
                    PercentMore, 50, Attribute.StrengthDamageBonus
                },
                {
                    "({BuffMatchers})",
                    TotalOverride, 1, Reference.AsBuff.On(Self)
                },
                {
                    "modifiers to ({KeywordMatchers}) damage apply to this skill's damage over time effect",
                    TotalOverride, 1, Stat.DamageHasKeyword(DamageSource.OverTime, Reference.AsKeyword)
                },
                {
                    "modifiers to spell damage apply to this skill's damage over time effect",
                    TotalOverride, 100,
                    Damage.With(DamageSource.Spell)
                        .ApplyModifiersToSkills(DamageSource.OverTime, Form.Increase, Form.More)
                },
                {
                    "increases and reductions to spell damage also apply to attacks",
                    TotalOverride, 100,
                    Damage.With(DamageSource.Attack).ApplyModifiersToSkills(DamageSource.Spell, Form.Increase)
                },
                {
                    "increases and reductions to minion damage also affect you",
                    TotalOverride, 1, Flag.IncreasesToSourceApplyToTarget(Damage.For(Entity.Minion), Damage)
                },
                {
                    "increases and reductions to minion attack speed also affect you",
                    TotalOverride, 1,
                    Flag.IncreasesToSourceApplyToTarget(Stat.CastRate.With(DamageSource.Attack).For(Entity.Minion),
                        Stat.CastRate.With(DamageSource.Attack))
                },
                {
                    "increases and reductions to cast speed also apply to this skill's activation frequency",
                    TotalOverride, 1,
                    Flag.IncreasesToSourceApplyToTarget(Stat.CastRate.With(DamageSource.Spell), Stat.HitRate)
                },
                {
                    "({StatMatchers}) is doubled",
                    PercentMore, 100, Reference.AsStat
                },
                {
                    "gain #% of maximum ({PoolStatMatchers}) as extra maximum energy shield",
                    BaseAdd, Value, Reference.AsPoolStat.ConvertTo(EnergyShield)
                },
                {
                    "(your )?damaging hits always stun enemies that are on full life",
                    TotalOverride, 100, Effect.Stun.Chance,
                    Action.Unique("On damaging Hit against a full life Enemy").On
                },
                {
                    "maximum # stages",
                    BaseSet, Value, Stat.SkillStage.Maximum
                },
                // Jewels
                {
                    "primordial",
                    BaseAdd, 1, Stat.PrimordialJewelsSocketed
                },
                {
                    "grand spectrum",
                    BaseAdd, 1, Stat.GrandSpectrumJewelsSocketed
                },
                {
                    // Brute Force Solution, Careful Planning, Efficient Training, Fertile Mind, Fluid Motion, Inertia
                    "({AttributeStatMatchers}) from passives in radius is transformed to ({AttributeStatMatchers})",
                    (BaseSubtract, 1,
                        PassiveTree.MultipliedAttributeForNodesInModifierSourceJewelRadius(References[0].AsStat,
                            References[0].AsStat)),
                    (BaseAdd, 1,
                        PassiveTree.MultipliedAttributeForNodesInModifierSourceJewelRadius(References[0].AsStat,
                            References[1].AsStat))
                },
                {
                    // Combat Focus
                    "with # total ({AttributeStatMatchers}) and ({AttributeStatMatchers}) in radius, elemental hit cannot choose fire",
                    TotalOverride, 0, Fire.Damage, CombatFocusCondition(0)
                },
                {
                    "with # total ({AttributeStatMatchers}) and ({AttributeStatMatchers}) in radius, elemental hit cannot choose cold",
                    TotalOverride, 0, Cold.Damage, CombatFocusCondition(1)
                },
                {
                    "with # total ({AttributeStatMatchers}) and ({AttributeStatMatchers}) in radius, elemental hit cannot choose lightning",
                    TotalOverride, 0, Lightning.Damage, CombatFocusCondition(2)
                },
                {
                    // Intuitive Leap
                    "passives in radius can be allocated without being connected to your tree",
                    TotalOverride, 1, PassiveTree.ConnectJewelToNodesInModifierSourceJewelRadius
                },
                {
                    // Might in All Forms
                    "({AttributeStatMatchers}) and ({AttributeStatMatchers}) from passives in radius count towards strength melee damage bonus",
                    (BaseAdd, PassiveTree.AllocatedInModifierSourceJewelRadius(References[0].AsStat),
                        Attribute.StrengthDamageBonus),
                    (BaseAdd, PassiveTree.AllocatedInModifierSourceJewelRadius(References[1].AsStat),
                        Attribute.StrengthDamageBonus)
                },
                {
                    // Might of the Meek
                    "#% increased effect of non-keystone passive skills in radius",
                    PercentIncrease, Value, PassiveTree.ModifyNodeEffectivenessInModifierSourceJewelRadius(false,
                        PassiveNodeType.Small, PassiveNodeType.Notable)
                },
                {
                    "notable passive skills in radius grant nothing",
                    TotalOverride, 0, PassiveTree.ModifyNodeEffectivenessInModifierSourceJewelRadius(false,
                        PassiveNodeType.Notable)
                },
                {
                    // Soul's Wick
                    "spectres have a base duration of # seconds spectres do not travel between areas",
                    BaseSet, Value, Stat.Duration, With(Skills.RaiseSpectre)
                },
                {
                    // The Vigil
                    "with at least # strength in radius, ({SkillMatchers}) fortifies you and nearby allies for # seconds",
                    TotalOverride, 1, Buff.Fortify.On(Self).Concat(Buff.Fortify.On(Ally)),
                    And(PassiveTree.TotalInModifierSourceJewelRadius(Attribute.Strength) >= Values[0],
                        Reference.AsSkill.Cast.InPastXSeconds(Values[1]))
                },
                {
                    // Unnatural Instinct
                    "grants all bonuses of unallocated small passive skills in radius",
                    BaseAdd, 1, PassiveTree.ModifyNodeEffectivenessInModifierSourceJewelRadius(false,
                        PassiveNodeType.Small)
                },
                {
                    "allocated small passive skills in radius grant nothing",
                    TotalOverride, 0, PassiveTree.ModifyNodeEffectivenessInModifierSourceJewelRadius(true,
                        PassiveNodeType.Small)
                },
                // skills
                {
                    // Dread Banner, War Banner
                    @"\+# second to base placed banner duration per stage",
                    BaseAdd, Value * Stat.BannerStage.Value, Stat.Duration
                },
                {
                    // Chain Hook
                    @"shockwave has \+# radius per # rage",
                    BaseAdd, Values[0] * (Charge.Rage.Amount.Value / Values[1]).Floor(), Stat.Radius
                },
                {
                    // Cleave
                    "when dual wielding, deals #% damage from each weapon combined",
                    PercentLess, 100 - Value, Damage, OffHand.Has(Tags.Weapon)
                },
                {
                    // Cyclone
                    "modifiers to melee attack range also apply to this skill's area radius",
                    BaseAdd, Stat.Range.With(Keyword.Melee).With(AttackDamageHand.MainHand).ValueFor(NodeType.BaseAdd),
                    Stat.Radius
                },
                {
                    // Dash
                    "this skill's cast speed cannot be modified",
                    TotalOverride, Stat.CastRate.With(DamageSource.Spell).ValueFor(NodeType.BaseSet),
                    Stat.CastRate.With(DamageSource.Spell)
                },
                {
                    // Freeze Mine
                    "enemies lose #% cold resistance while frozen",
                    BaseSubtract, Value, Cold.Resistance.For(Enemy), Ailment.Freeze.IsOn(Enemy)
                },
                {
                    // Infernal Blow
                    "debuff deals #% of damage per charge",
                    BaseSet, Value.AsPercentage * Stat.SkillStage.Value *
                             Physical.Damage.WithSkills.With(AttackDamageHand.MainHand).ValueFor(NodeType.Base),
                    Fire.Damage.WithSkills(DamageSource.Secondary)
                },
                {
                    // Scorching Ray
                    "burning debuff can have a maximum of # stages",
                    TotalOverride, Value, Skills.ModifierSourceSkill.Buff.StackCount.Maximum.For(Enemy)
                },
                {
                    "additional debuff stages add #% of damage",
                    PercentMore,
                    ValueBuilderUtils.PerStatAfterFirst(Skills.ModifierSourceSkill.Buff.StackCount.For(Enemy))(Value),
                    Damage
                },
                {
                    // Static Strike
                    "#% increased beam frequency per buff stack",
                    PercentIncrease, Value * Stat.SkillStage.Value, Stat.HitRate
                },
                {
                    // Swift Affliction Support
                    "#% reduced duration of supported skills and damaging ailments they inflict",
                    PercentReduce, Value,
                    ApplyOnce(Stat.Duration, Stat.SecondaryDuration,
                        Ailment.Ignite.Duration, Ailment.Bleed.Duration, Ailment.Poison.Duration)
                },
                {
                    // Vaal Ground Slam
                    "stuns enemies",
                    TotalOverride, 100, Effect.Stun.Chance
                },
                {
                    // Vaal Impurity of Ice/Fire/Lightning
                    "nearby enemies' ({DamageTypeMatchers}) resistance is ignored by hits",
                    TotalOverride, 1, Reference.AsDamageType.IgnoreResistance
                },
                {
                    // Vaal Rain of Arrows
                    "maim on hit",
                    TotalOverride, 100, Buff.Maim.Chance.WithHits
                },
                {
                    // Winter Orb
                    "#% increased projectile frequency per stage",
                    PercentIncrease, Value * Stat.SkillStage.Value, Stat.HitRate
                },
                {
                    "increases and reductions to cast speed also apply to projectile frequency",
                    TotalOverride, 1,
                    Flag.IncreasesToSourceApplyToTarget(Stat.CastRate.With(DamageSource.Spell), Stat.HitRate)
                },
                {
                    // Blasphemy Support
                    "using supported skills is instant",
                    TotalOverride, 0, Stat.BaseCastTime
                },
                {
                    // Fork Support
                    "supported skills fork",
                    TotalOverride, 1, Projectile.Fork
                },
                {
                    // Iron Will Support
                    "strength's damage bonus applies to spell damage as well for supported skills",
                    PercentIncrease,
                    (Attribute.StrengthDamageBonus.Value / 5).Ceiling(),
                    Damage.WithSkills(DamageSource.Spell)
                },
                {
                    // Minion and Totem Elemental Resistance Support
                    @"totems and minions summoned by supported skills have \+#% ({DamageTypeMatchers}) resistance",
                    BaseAdd, Value, Reference.AsDamageType.Resistance.For(Entity.Minion),
                    Reference.AsDamageType.Resistance.For(Entity.Totem)
                },
                {
                    // Multistrike Support
                    "(first|second) repeat of supported skills deals #% more damage",
                    PercentMore, Value / 3, Damage
                },
                {
                    // Unleash Support
                    "supported spells gain a seal every # seconds, to a maximum of # seals " +
                    "supported spells are unsealed when cast, and their effects reoccur for each seal lost",
                    TotalOverride, Values[0].Invert, Stat.AdditionalCastRate
                },
                {
                    "supported skills deal #% less damage when reoccurring",
                    (PercentLess, Values[0] * Stat.AdditionalCastRate.Value
                                  / (Stat.AdditionalCastRate.Value + Stat.CastRate.With(DamageSource.Spell).Value),
                        Damage.With(DamageSource.Spell)),
                    (PercentLess, Values[0] * Stat.AdditionalCastRate.Value
                                  / (Stat.AdditionalCastRate.Value + Stat.CastRate.With(DamageSource.Secondary).Value),
                        Damage.With(DamageSource.Secondary))
                },
                // Keystones
                {
                    // Point Blank
                    "projectile attack hits deal up to #% more damage to targets at the start of their movement, " +
                    "dealing less damage to targets as the projectile travels farther",
                    PercentMore,
                    // 0 to 10: Value; 10 to 35: Value to 0; 35 to 150: 0 to -Value
                    Value * ValueFactory.LinearScale(Projectile.TravelDistance, (0, 1), (10, 1), (35, 0), (150, -1)),
                    Damage.WithSkills(DamageSource.Attack).With(Keyword.Projectile)
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
                    // Runebinder
                    "you can have an additional brand attached to an enemy",
                    BaseAdd, 1, Stat.AttachedBrands.For(Enemy).Maximum
                },
                {
                    // Blood Magic
                    "spend life instead of mana for skills",
                    (BaseAdd, 100, Mana.Cost.ConvertTo(Life.Cost), Condition.True),
                    (TotalOverride, (int) Pool.Life, AllSkills.ReservationPool, Condition.True)
                },
                {
                    // Mortal Conviction
                    "you can only have one non-banner aura on you from your skills non-banner aura skills reserve no mana",
                    TotalOverride, 0, Skills[Keyword.Aura].Reservation
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
                    "action speed cannot be modified to below base value",
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
                {
                    "effects granted for having rage are tripled",
                    PercentMore, 200, Charge.RageEffect
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
                { "far shot", TotalOverride, 1, Flag.FarShot },
                {
                    // Ascendant
                    "projectiles gain damage as they travel f(u|a)rther, dealing up to #% increased damage with hits to targets",
                    PercentIncrease,
                    Value * ValueFactory.LinearScale(Projectile.TravelDistance, (35, 0), (70, 1)),
                    Damage.WithHits.With(Keyword.Projectile)
                },
                { "accuracy rating is doubled", PercentMore, 100, Stat.Accuracy },
                {
                    "if you've used a skill recently, you and nearby allies have tailwind",
                    TotalOverride, 1, Buff.Tailwind.On(Self).CombineWith(Buff.Tailwind.On(Ally)),
                    AllSkills.Cast.Recently
                },
                // - Pathfinder
                {
                    "poisons you inflict during any flask effect have #% chance to deal #% more damage",
                    PercentMore, Values[0].AsPercentage * Values[1], Damage.With(Ailment.Poison)
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
                    TotalOverride, 100, Elemental.Resistance.For(Entity.Minion),
                    And(With(Keyword.Golem), Or(With(Fire), With(Cold), With(Lightning)))
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
                    TotalOverride, 100, Buff.Maim.Chance.With(Keyword.Attack), Ailment.Bleed.IsOn(Enemy)
                },
                {
                    "your counterattacks deal double damage",
                    TotalOverride, 100, Damage.With(Keyword.CounterAttack).ChanceToDouble
                },
                {
                    "Chance to Block Spell Damage is equal to Chance to Block Attack Damage Maximum Chance to Block Spell Damage is equal to Maximum Chance to Block Attack Damage",
                    (TotalOverride, Block.AttackChance.Value, Block.SpellChance),
                    (TotalOverride, Block.AttackChance.Maximum.Value, Block.SpellChance.Maximum)
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
                {
                    "impales you inflict last # additional hits",
                    BaseAdd, Value, Buff.Impale.StackCount.For(Enemy).Maximum
                },
                {
                    "banner skills reserve no mana",
                    TotalOverride, 0, Skills[Keyword.Banner].Reservation
                },
                {
                    "you and allies affected by your placed banners regenerate #% of maximum life per second for each stage",
                    BaseAdd, Value * Stat.BannerStage.Value, Life.Regen.Percent,
                    Or(For(Self), And(For(Ally), Buffs(targets: Ally).With(Keyword.Banner).Any()))
                },
                // - Slayer
                { "cannot take reflected physical damage", PercentLess, 100, Physical.ReflectedDamageTaken },
                {
                    "base critical strike chance for attacks with weapons is #%",
                    TotalOverride, Value, CriticalStrike.BaseChance.WithSkills(DamageSource.Attack), MainHand.HasItem
                },
                {
                    "deal up to #% more melee damage to enemies, based on proximity",
                    PercentMore, Stat.UniqueAmount("#% more Melee Damage from Slayer's Impact"),
                    Damage.With(Keyword.Melee)
                },
                {
                    "your maximum endurance charges is equal to your maximum frenzy charges",
                    TotalOverride, Charge.Frenzy.Amount.Maximum.Value, Charge.Endurance.Amount.Maximum
                },
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
                    "enemies take #% increased damage for each of your brands attached to them",
                    PercentIncrease, Value * Stat.AttachedBrands.For(Enemy).Maximum.Value, Damage.Taken.For(Enemy)
                },
                // - Guardian
                {
                    "grants armour equal to #% of your reserved life to you and nearby allies",
                    BaseAdd,
                    Value.AsPercentage * Life.Reservation.Value, Buff.Buff(Armour, Self, Ally)
                },
                {
                    "grants maximum energy shield equal to #% of your reserved mana to you and nearby allies",
                    BaseAdd,
                    Value.AsPercentage * Mana.Reservation.Value, Buff.Buff(EnergyShield, Self, Ally)
                },
                {
                    "warcries cost no mana",
                    TotalOverride, 0, Mana.Cost, With(Keyword.Warcry)
                },
                {
                    "using warcries is instant",
                    TotalOverride, 0, Stat.BaseCastTime, With(Keyword.Warcry)
                },
                {
                    @"\+#% chance to block attack damage for # seconds every # seconds",
                    BaseAdd, Values[0], Block.AttackChance,
                    Condition.Unique("Is the additional Block Chance from Bastion of Hope active?")
                },
                {
                    "minions intimidate enemies for # seconds on hit",
                    TotalOverride, 1, Buff.Intimidate.On(Enemy), Action.Hit.By(Entity.Minion).Recently
                },
                // - Assassin
                {
                    // Ascendant
                    "your critical strikes with attacks maim enemies",
                    TotalOverride, 1, Buff.Maim.On(Enemy),
                    And(Condition.WithPart(Keyword.Attack), CriticalStrike.On)
                },
                // - Trickster
                { "movement skills cost no mana", TotalOverride, 0, Mana.Cost, With(Keyword.Movement) },
                {
                    "#% chance to gain #% of non-chaos damage with hits as extra chaos damage",
                    BaseAdd, Values[0] * Values[1] / 100, Chaos.Invert.Damage.WithHits.GainAs(Chaos.Damage.WithHits)
                },
                // - Saboteur
                { "nearby enemies are blinded", TotalOverride, 1, Buff.Blind.On(Enemy), Enemy.IsNearby },
                // - Ascendant (generic)
                {
                    "can allocate passives from the marauder's starting point",
                    TotalOverride, 1, PassiveTree.ConnectsToClass(CharacterClass.Marauder)
                },
                {
                    "can allocate passives from the duelist's starting point",
                    TotalOverride, 1, PassiveTree.ConnectsToClass(CharacterClass.Duelist)
                },
                {
                    "can allocate passives from the ranger's starting point",
                    TotalOverride, 1, PassiveTree.ConnectsToClass(CharacterClass.Ranger)
                },
                {
                    "can allocate passives from the shadow's starting point",
                    TotalOverride, 1, PassiveTree.ConnectsToClass(CharacterClass.Shadow)
                },
                {
                    "can allocate passives from the witch's starting point",
                    TotalOverride, 1, PassiveTree.ConnectsToClass(CharacterClass.Witch)
                },
                {
                    "can allocate passives from the templar's starting point",
                    TotalOverride, 1, PassiveTree.ConnectsToClass(CharacterClass.Templar)
                },
            };

        private IConditionBuilder CombatFocusCondition(int skillPart)
            => And(With(Skills.FromId("ElementalHit")), Stat.MainSkillPart.Value.Eq(skillPart),
                (PassiveTree.TotalInModifierSourceJewelRadius(References[0].AsStat)
                 + PassiveTree.TotalInModifierSourceJewelRadius(References[1].AsStat))
                >= Value);

        private IEnumerable<(IFormBuilder form, IValueBuilder value, IStatBuilder stat)>
            UndeniableAttackSpeed()
        {
            var attackSpeed = Stat.CastRate.With(DamageSource.Attack);
            foreach (var hand in Enums.GetValues<AttackDamageHand>())
            {
                IValueBuilder PerAccuracy(ValueBuilder value) =>
                    ValueBuilderUtils.PerStat(Stat.Accuracy.With(hand), Values[1])(value);

                yield return (PercentIncrease, PerAccuracy(Values[0]), attackSpeed.With(hand));
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

        private (IFormBuilder form, double value, IStatBuilder stat)[]
            ShaperOfDesolation()
        {
            return new[]
            {
                Buff.Temporary(Buff.Conflux.Chilling, ShaperOfDesolationStep.Chilling),
                Buff.Temporary(Buff.Conflux.Shocking, ShaperOfDesolationStep.Shocking),
                Buff.Temporary(Buff.Conflux.Igniting, ShaperOfDesolationStep.Igniting),
                Buff.Temporary(Buff.Conflux.Chilling, ShaperOfDesolationStep.All),
                Buff.Temporary(Buff.Conflux.Shocking, ShaperOfDesolationStep.All),
                Buff.Temporary(Buff.Conflux.Igniting, ShaperOfDesolationStep.All),
            }.Select(s => (TotalOverride, 1.0, s)).ToArray();
        }

        private enum ShaperOfDesolationStep
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

        private enum PendulumOfDestructionStep
        {
            None,
            AreaOfEffect,
            ElementalDamage
        }
    }
}