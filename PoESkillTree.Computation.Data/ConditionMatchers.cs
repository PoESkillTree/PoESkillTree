using System.Collections.Generic;
using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;

namespace PoESkillTree.Computation.Data
{
    /// <inheritdoc />
    /// <summary>
    /// <see cref="IStatMatchers"/> implementation matching stat parts specifying conditions.
    /// </summary>
    public class ConditionMatchers : StatMatchersBase
    {
        private readonly IModifierBuilder _modifierBuilder;

        public ConditionMatchers(
            IBuilderFactories builderFactories, IMatchContexts matchContexts, IModifierBuilder modifierBuilder)
            : base(builderFactories, matchContexts)
        {
            _modifierBuilder = modifierBuilder;
        }

        protected override IEnumerable<MatcherData> CreateCollection() =>
            new ConditionMatcherCollection(_modifierBuilder)
            {
                // actions
                // - generic
                { "on ({ActionMatchers})", Reference.AsAction.On() },
                { "if you've ({ActionMatchers}) recently", Reference.AsAction.Recently },
                { "if you haven't ({ActionMatchers}) recently", Not(Reference.AsAction.Recently) },
                {
                    "when you ({ActionMatchers}) a rare or unique enemy",
                    And(Reference.AsAction.Against(Enemy).On(), Enemy.IsRareOrUnique)
                },
                // - kill
                { "on ({KeywordMatchers}) kill", Kill.On(Reference.AsKeyword) },
                { "when you kill an enemy,", Kill.Against(Enemy).On() },
                {
                    "if you've killed a maimed enemy recently",
                    And(Kill.Against(Enemy).Recently, Buff.Maim.IsOn(Enemy))
                },
                {
                    "if you've killed a bleeding enemy recently",
                    And(Kill.Against(Enemy).Recently, Ailment.Bleed.IsOn(Enemy))
                },
                {
                    "if you've killed a cursed enemy recently",
                    And(Kill.Against(Enemy).Recently, Buffs(target: Enemy).With(Keyword.Curse).Any())
                },
                {
                    "if you or your totems have killed recently",
                    Or(Kill.Recently, Kill.By(Entity.Totem).Recently)
                },
                { "if you or your totems kill an enemy", Or(Kill.On(), Kill.By(Entity.Totem).On()) },
                // - block
                { "when they block", Block.On() },
                { "when you block", Block.On() },
                {
                    "if you've blocked a hit from a unique enemy recently",
                    And(Block.Against(Enemy).Recently, Enemy.IsUnique)
                },
                // - hit
                { "(from|with) hits", Hit.On() },
                { "hits deal", Hit.On() },
                { "when you are hit", Hit.Taken.On() },
                { "if you've been hit recently", Hit.Taken.Recently },
                { "if you haven't been hit recently", Not(Hit.Taken.Recently) },
                { "if you were damaged by a hit recently", Hit.Taken.Recently },
                { "if you've taken no damage from hits recently", Not(Hit.Taken.Recently) },
                // - other
                { "if you've taken a savage hit recently", Action.SavageHit.Taken.Recently },
                { "when you deal a critical strike", CriticalStrike.On() },
                { "if you've crit in the past # seconds", CriticalStrike.InPastXSeconds(Value) },
                { "if you've shattered an enemy recently", Action.Shatter.Against(Enemy).Recently },
                { "when you stun an enemy", Effect.Stun.On() },
                { "after spending # mana", Action.SpendMana(Value).On() },
                { "if you have consumed a corpse recently", Action.ConsumeCorpse.Recently },
                { "when you gain a ({ChargeTypeMatchers})", Reference.AsChargeType.GainAction.On() },
                // damage
                // - by item tag
                { "with weapons", Damage.With(Tags.Weapon) },
                { "weapon", Damage.With(Tags.Weapon) },
                { "with bows", Damage.With(Tags.Bow) },
                { "bow", Damage.With(Tags.Bow) },
                { "with swords", Damage.With(Tags.Sword) },
                { "with claws", Damage.With(Tags.Claw) },
                { "claw", Damage.With(Tags.Claw) },
                { "with daggers", Damage.With(Tags.Dagger) },
                { "with wands", Damage.With(Tags.Wand) },
                { "wand", Damage.With(Tags.Wand) },
                { "with axes", Damage.With(Tags.Axe) },
                { "with staves", Damage.With(Tags.Staff) },
                { "with maces", Or(Damage.With(Tags.Mace), Damage.With(Tags.Sceptre)) },
                { "with one handed weapons", Damage.With(Tags.OneHandWeapon) },
                {
                    "with one handed melee weapons",
                    And(Damage.With(Tags.OneHandWeapon), Not(Damage.With(Tags.Ranged)))
                },
                { "with two handed weapons", Damage.With(Tags.TwoHandWeapon) },
                {
                    "with two handed melee weapons",
                    And(Damage.With(Tags.TwoHandWeapon), Not(Damage.With(Tags.Ranged)))
                },
                // - by item slot
                { "with the main-hand weapon", Damage.With(ItemSlot.MainHand) },
                { "with main hand", Damage.With(ItemSlot.MainHand) },
                { "with off hand", Damage.With(ItemSlot.OffHand) },
                // - by source
                { "attacks have", Damage.With(Source.Attack()) },
                { "with attacks", Damage.With(Source.Attack()) },
                { "from damage over time", Damage.With(Source.OverTime()) },
                // - by ailment
                { "with ({AilmentMatchers})", Damage.With(Reference.AsAilment) },
                { "with ailments", Ailment.All.Any(Damage.With) },
                // action and damage combinations
                // - by item tag
                { "if you get a critical strike with a bow", And(CriticalStrike.On(), Damage.With(Tags.Bow)) },
                { "if you get a critical strike with a staff", And(CriticalStrike.On(), Damage.With(Tags.Staff)) },
                { "critical strikes with daggers have a", And(CriticalStrike.On(), Damage.With(Tags.Dagger)) },
                // - by item slot
                // - by source
                { "for each enemy hit by your attacks", And(Hit.Against(Enemy).On(), Damage.With(Source.Attack())) },
                // - by ailment
                { "with hits and ailments", Or(Hit.On(), Ailment.All.Any(Damage.With)) },
                {
                    "poison you inflict with critical strikes deals",
                    And(Damage.With(Ailment.Poison), CriticalStrike.On())
                },
                // equipment
                { "while unarmed", Not(MainHand.HasItem) },
                { "while wielding a staff", MainHand.Has(Tags.Staff) },
                { "while wielding a dagger", MainHand.Has(Tags.Dagger) },
                { "while wielding a bow", MainHand.Has(Tags.Bow) },
                { "while wielding a sword", MainHand.Has(Tags.Sword) },
                { "while wielding a claw", MainHand.Has(Tags.Claw) },
                { "while wielding an axe", MainHand.Has(Tags.Axe) },
                { "while wielding a mace", Or(MainHand.Has(Tags.Mace), MainHand.Has(Tags.Sceptre)) },
                { "while wielding a melee weapon", And(MainHand.Has(Tags.Weapon), Not(MainHand.Has(Tags.Ranged))) },
                { "while wielding a one handed weapon", MainHand.Has(Tags.OneHandWeapon) },
                { "while wielding a two handed weapon", MainHand.Has(Tags.TwoHandWeapon) },
                { "while dual wielding", OffHand.Has(Tags.Weapon) },
                { "while holding a shield", OffHand.Has(Tags.Shield) },
                { "while dual wielding or holding a shield", Or(OffHand.Has(Tags.Weapon), OffHand.Has(Tags.Shield)) },
                { "with shields", OffHand.Has(Tags.Shield) },
                { "from equipped shield", And(Condition.BaseValueComesFrom(ItemSlot.OffHand), OffHand.Has(Tags.Shield)) },
                { "with # corrupted items equipped", Equipment.Count(e => e.IsCorrupted) >= Value },
                // stats
                // - pool
                { "when on low life", Life.IsLow },
                { "when not on low life", Not(Life.IsLow) },
                { "while no mana is reserved", Mana.Reservation.Value <= 0 },
                { "while energy shield is full", EnergyShield.IsFull },
                { "while on full energy shield", EnergyShield.IsFull },
                { "while not on full energy shield", Not(EnergyShield.IsFull) },
                { "if energy shield recharge has started recently", EnergyShield.Recharge.StartedRecently },
                // - charges
                { "while you have no ({ChargeTypeMatchers})", Reference.AsChargeType.Amount.Value <= 0 },
                {
                    "while (at maximum|on full) ({ChargeTypeMatchers})",
                    Reference.AsChargeType.Amount.Value >= Reference.AsChargeType.Amount.Maximum.Value
                },
                // - other
                { "if you have # primordial jewels,", Stat.PrimordialJewelsSocketed.Value >= Value },
                // - on enemy
                { "(against enemies )?that are on low life", Life.For(Enemy).IsLow },
                { "(against enemies )?that are on full life", Life.For(Enemy).IsFull },
                { "against rare and unique enemies", Enemy.IsRareOrUnique },
                // buffs
                { "while you have ({BuffMatchers})", Reference.AsBuff.IsOn(Self) },
                { "during onslaught", Buff.Onslaught.IsOn(Self) },
                { "while phasing", Buff.Phasing.IsOn(Self) },
                { "if you've taunted an enemy recently", Buff.Taunt.Action.Recently },
                { "enemies you taunt( deal| take)?", And(For(Enemy), Buff.Taunt.IsOn(Enemy)) },
                { "enemies you curse (take|have)", And(For(Enemy), Buffs(Self, Enemy).With(Keyword.Curse).Any()) },
                { "(against|from) blinded enemies", Buff.Blind.IsOn(Enemy) },
                { "from taunted enemies", Buff.Taunt.IsOn(Enemy) },
                {
                    "you and allies affected by your auras have",
                    Or(For(Entity.ModifierSource), And(For(Ally), Buffs(target: Ally).With(Keyword.Aura).Any()))
                },
                {
                    "you and allies deal while affected by auras you cast",
                    Or(For(Entity.ModifierSource), And(For(Ally), Buffs(target: Ally).With(Keyword.Aura).Any()))
                },
                // buff and damage combinations
                {
                    "bleeding you inflict on maimed enemies deals",
                    And(Damage.With(Ailment.Bleed), Buff.Maim.IsOn(Enemy))
                },
                // ailments
                { "while ({AilmentMatchers})", Reference.AsAilment.IsOn(Self) },
                { "(against|from) ({AilmentMatchers}) enemies", Reference.AsAilment.IsOn(Enemy) },
                {
                    "against frozen, shocked or ignited enemies",
                    Or(Ailment.Freeze.IsOn(Enemy), Ailment.Shock.IsOn(Enemy), Ailment.Ignite.IsOn(Enemy))
                },
                { "enemies which are ({AilmentMatchers})", Reference.AsAilment.IsOn(Enemy) },
                {
                    "against enemies( that are)? affected by elemental ailments",
                    Ailment.Elemental.Any(a => a.IsOn(Enemy))
                },
                {
                    "against enemies( that are)? affected by no elemental ailments",
                    Not(Ailment.Elemental.Any(a => a.IsOn(Enemy)))
                },
                // ground effects
                { "while on consecrated ground", Ground.Consecrated.IsOn(Self) },
                // other effects
                { "against burning enemies", Fire.DamageOverTimeIsOn(Enemy) },
                // skills
                // - by keyword
                { "vaal( skill)?", With(Keyword.Vaal) },
                { "({KeywordMatchers})", With(Reference.AsKeyword) },
                {
                    "({KeywordMatchers}) and ({KeywordMatchers})",
                    Or(With(References[0].AsKeyword), With(References[1].AsKeyword))
                },
                { "(with|of|for|from) ({KeywordMatchers})( skills)?", With(Reference.AsKeyword) },
                { "({KeywordMatchers}) skills (have|deal)", With(Reference.AsKeyword) },
                // - by damage type
                { "with ({DamageTypeMatchers}) skills", With(Reference.AsDamageType) },
                // - by single skill
                { "({SkillMatchers})('|s)?( fires| has a| have a| has| deals| gain)?", With(Reference.AsSkill) },
                { "(dealt by) ({SkillMatchers})", With(Reference.AsSkill) },
                // - cast recently/in past x seconds
                { "if you've cast a spell recently", Skills[Keyword.Spell].Cast.Recently },
                { "if you've attacked recently", Skills[Keyword.Attack].Cast.Recently },
                { "if you've used a movement skill recently", Skills[Keyword.Movement].Cast.Recently },
                { "if you've used a warcry recently", Skills[Keyword.Warcry].Cast.Recently },
                {
                    "if you've used a ({DamageTypeMatchers}) skill in the past # seconds",
                    Skills[Reference.AsDamageType].Cast.InPastXSeconds(Value)
                },
                // skill and action combinations
                {
                    "projectiles have against targets they pierce",
                    And(Projectile.Pierce.On(), With(Keyword.Projectile))
                },
                // traps and mines
                { "with traps", With(Keyword.Trap) },
                { "with mines", With(Keyword.Mine) },
                { "traps and mines (deal|have a)", Or(With(Keyword.Trap), With(Keyword.Mine)) },
                { "for throwing traps", With(Keyword.Trap) },
                { "if you detonated mines recently", Skill.DetonateMines.Cast.Recently },
                { "if you've placed a mine or thrown a trap recently", Or(Traps.Cast.Recently, Mines.Cast.Recently) },
                // totems
                { "totems", For(Entity.Totem) },
                { "totems (gain|have)", For(Entity.Totem) },
                { "totems fire", With(Keyword.Totem) },
                { "(spells cast|attacks used|skills used) by totems (have a|have)", With(Keyword.Totem) },
                { "of totem skills that cast an aura", And(With(Keyword.Totem), With(Keyword.Aura)) },
                { "while you have a totem", Totems.Any(s => s.HasInstance) },
                { "if you've summoned a totem recently", Totems.Cast.Recently },
                { "when you place a totem", Totems.Cast.On() },
                // minions
                { "minions", For(Entity.Minion) },
                { "minions (deal|have|gain)", For(Entity.Minion) },
                { "you and your minions have", For(Entity.Minion, Entity.ModifierSource) },
                { "golem", For(Entity.Minion.With(Keyword.Golem)) },
                { "golems have", For(Entity.Minion.With(Keyword.Golem)) },
                { "spectres have", For(Entity.Minion.From(Skill.RaiseSpectre)) },
                {
                    // Technically this would be separate for each minion summoned by that skill, but DPS will 
                    // only be calculated for a single minion anyway.
                    "golems summoned in the past # seconds deal",
                    And(With(Keyword.Golem), Skill.MainSkill.Cast.InPastXSeconds(Value))
                },
                { "if you Summoned a golem in the past # seconds", Golems.Cast.InPastXSeconds(Value) },
                // flasks
                { "while using a flask", Flask.IsAnyActive },
                { "during any flask effect", Flask.IsAnyActive },
                // other
                { "(you )?gain", Condition.True }, // may be left over at the end, does nothing
                // unique
                { "while leeching", Condition.Unique("Are you leeching?") },
                {
                    "when your trap is triggered by an enemy",
                    Condition.Unique("When your Trap is triggered by an Enemy")
                },
                {
                    "when your mine is detonated targeting an enemy",
                    Condition.Unique("When your Mine is detonated targeting an Enemy")
                },
                {
                    "if you've killed an enemy affected by your damage over time recently",
                    Condition.Unique("Have you recently killed an Enemy affected by your Damage over Time?")
                },
            };
    }
}