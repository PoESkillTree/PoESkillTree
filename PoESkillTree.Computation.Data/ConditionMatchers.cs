using System.Collections.Generic;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.GameModel.Items;

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
                { "if you've ({ActionMatchers}) recently", Reference.AsAction.Recently },
                { "if you haven't ({ActionMatchers}) recently", Not(Reference.AsAction.Recently) },
                { "for # seconds on ({ActionMatchers})", Reference.AsAction.InPastXSeconds(Value) },
                {
                    "for # seconds when you ({ActionMatchers}) a rare or unique enemy",
                    And(Enemy.IsRareOrUnique, Reference.AsAction.InPastXSeconds(Value))
                },
                { "when you ({ActionMatchers}) an enemy, for # seconds", Reference.AsAction.InPastXSeconds(Value) },
                // - kill
                {
                    "if you've killed a maimed enemy recently",
                    And(Kill.Recently, Buff.Maim.IsOn(Enemy))
                },
                {
                    "if you've killed a bleeding enemy recently",
                    And(Kill.Recently, Ailment.Bleed.IsOn(Enemy))
                },
                {
                    "if you've killed a cursed enemy recently",
                    And(Kill.Recently, Buffs(targets: Enemy).With(Keyword.Curse).Any())
                },
                {
                    "if you or your totems have killed recently",
                    Or(Kill.Recently, Kill.By(Entity.Totem).Recently)
                },
                {
                    "if you or your minions have killed recently",
                    Or(Kill.Recently, Kill.By(Entity.Minion).Recently)
                },
                // - hit
                { "if you've been hit recently", Hit.By(Enemy).Recently },
                { "if you haven't been hit recently", Not(Hit.By(Enemy).Recently) },
                { "if you were damaged by a hit recently", Hit.By(Enemy).Recently },
                { "if you've taken no damage from hits recently", Not(Hit.By(Enemy).Recently) },
                // - critical strike
                { "if you've crit in the past # seconds", CriticalStrike.InPastXSeconds(Value) },
                { "if you've dealt a critical strike in the past # seconds", CriticalStrike.InPastXSeconds(Value) },
                // - block
                { "if you've blocked in the past # seconds,?", Block.InPastXSeconds(Value) },
                { "if you've blocked a hit from a unique enemy recently", And(Block.Recently, Enemy.IsUnique) },
                {
                    "if you've blocked a hit from a unique enemy in the past # seconds",
                    And(Block.InPastXSeconds(Value), Enemy.IsUnique)
                },
                // - other
                { "if you've taken a savage hit recently", Action.SavageHit.By(Enemy).Recently },
                { "if you've shattered an enemy recently", Action.Shatter.Recently },
                {
                    "for # seconds after spending( a total of)? # mana",
                    Action.SpendMana(Values[1]).InPastXSeconds(Values[0])
                },
                { "if you have consumed a corpse recently", Action.ConsumeCorpse.Recently },
                // damage
                { "attacks have", Condition.With(DamageSource.Attack) },
                { "with attacks", Condition.With(DamageSource.Attack) },
                { "for spells", Condition.With(DamageSource.Spell) },
                // - by item tag
                { "with weapons", AttackWith(Tags.Weapon) },
                { "weapon", AttackWith(Tags.Weapon) },
                { "with bows", AttackWith(Tags.Bow) },
                { "with a bow", AttackWith(Tags.Bow) },
                { "bow", AttackWith(Tags.Bow) },
                { "with swords", AttackWith(Tags.Sword) },
                { "with claws", AttackWith(Tags.Claw) },
                { "claw", AttackWith(Tags.Claw) },
                { "with daggers", AttackWith(Tags.Dagger) },
                { "with wands", AttackWith(Tags.Wand) },
                { "wand", AttackWith(Tags.Wand) },
                { "with axes", AttackWith(Tags.Axe) },
                { "with staves", AttackWith(Tags.Staff) },
                { "with a staff", AttackWith(Tags.Staff) },
                {
                    "with maces",
                    (Or(MainHandAttackWith(Tags.Mace), MainHandAttackWith(Tags.Sceptre)),
                        Or(OffHandAttackWith(Tags.Mace), OffHandAttackWith(Tags.Sceptre)))
                },
                { "with one handed weapons", AttackWith(Tags.OneHandWeapon) },
                {
                    "with one handed melee weapons",
                    (And(MainHandAttackWith(Tags.OneHandWeapon), Not(MainHand.Has(Tags.Ranged))),
                        And(OffHandAttackWith(Tags.OneHandWeapon), Not(OffHand.Has(Tags.Ranged))))
                },
                { "with two handed weapons", AttackWith(Tags.TwoHandWeapon) },
                {
                    "with two handed melee weapons",
                    And(MainHandAttackWith(Tags.TwoHandWeapon), Not(MainHand.Has(Tags.Ranged)))
                },
                // - by item slot
                { "with the main-hand weapon", Condition.AttackWith(AttackDamageHand.MainHand) },
                { "with main hand", Condition.AttackWith(AttackDamageHand.MainHand) },
                { "with off hand", Condition.AttackWith(AttackDamageHand.OffHand) },
                // - taken
                { "take", Condition.DamageTaken },
                // equipment
                { "while unarmed", Not(MainHand.HasItem) },
                { "while wielding a staff", MainHand.Has(Tags.Staff) },
                { "while wielding a dagger", EitherHandHas(Tags.Dagger) },
                { "while wielding a bow", MainHand.Has(Tags.Bow) },
                { "while wielding a sword", EitherHandHas(Tags.Sword) },
                { "while wielding a claw", EitherHandHas(Tags.Claw) },
                { "while wielding an axe", EitherHandHas(Tags.Axe) },
                { "while wielding a mace", Or(EitherHandHas(Tags.Mace), EitherHandHas(Tags.Sceptre)) },
                { "while wielding a wand", EitherHandHas(Tags.Wand) },
                { "while wielding a melee weapon", And(EitherHandHas(Tags.Weapon), Not(MainHand.Has(Tags.Ranged))) },
                { "while wielding a one handed weapon", MainHand.Has(Tags.OneHandWeapon) },
                { "while wielding a two handed weapon", MainHand.Has(Tags.TwoHandWeapon) },
                { "while dual wielding", OffHand.Has(Tags.Weapon) },
                { "while holding a shield", OffHand.Has(Tags.Shield) },
                { "while dual wielding or holding a shield", Or(OffHand.Has(Tags.Weapon), OffHand.Has(Tags.Shield)) },
                { "with shields", OffHand.Has(Tags.Shield) },
                {
                    "from equipped shield",
                    And(Condition.BaseValueComesFrom(ItemSlot.OffHand), OffHand.Has(Tags.Shield))
                },
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
                { "while you have an? ({ChargeTypeMatchers})", Reference.AsChargeType.Amount.Value > 0 },
                { "while you have at least # ({ChargeTypeMatchers})", Reference.AsChargeType.Amount.Value >= Value },
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
                { "while there is only one nearby enemy", Enemy.CountNearby.Eq(1) },
                // buffs
                { "while you have ({BuffMatchers})", Reference.AsBuff.IsOn(Self) },
                { "while affected by ({SkillMatchers})", Reference.AsSkill.Buff.IsOn(Self) },
                { "during onslaught", Buff.Onslaught.IsOn(Self) },
                { "while phasing", Buff.Phasing.IsOn(Self) },
                { "if you've taunted an enemy recently", Buff.Taunt.Action.Recently },
                { "enemies you taunt( deal)?", And(For(Enemy), Buff.Taunt.IsOn(Self, Enemy)) },
                { "enemies taunted by you", And(For(Enemy), Buff.Taunt.IsOn(Self, Enemy)) },
                { "enemies maimed by you", And(For(Enemy), Buff.Maim.IsOn(Self, Enemy)) },
                { "enemies you curse( have)?", And(For(Enemy), Buffs(Self, Enemy).With(Keyword.Curse).Any()) },
                { "(against|from) blinded enemies", Buff.Blind.IsOn(Enemy) },
                { "from taunted enemies", Buff.Taunt.IsOn(Enemy) },
                {
                    "you and allies affected by your auras have",
                    Or(For(Self), And(For(Ally), Buffs(targets: Ally).With(Keyword.Aura).Any()))
                },
                {
                    "you and allies deal while affected by auras you cast",
                    Or(For(Self), And(For(Ally), Buffs(targets: Ally).With(Keyword.Aura).Any()))
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
                // skills
                // - by keyword
                { "vaal( skill)?", With(Keyword.Vaal) },
                { "(with|of|for) ({KeywordMatchers}) skills", With(Reference.AsKeyword) },
                { "({KeywordMatchers}) skills have", With(Reference.AsKeyword) },
                // - by damage type
                { "with ({DamageTypeMatchers}) skills", With(Reference.AsDamageType) },
                // - by single skill
                { "({SkillMatchers})", With(Reference.AsSkill) },
                { "({SkillMatchers})('|s)? (fires|has a|have a|has|deals|gain)", With(Reference.AsSkill) },
                { "dealt by ({SkillMatchers})", With(Reference.AsSkill) },
                {
                    "({SkillMatchers}) and ({SkillMatchers})",
                    Or(With(References[0].AsSkill), With(References[1].AsSkill))
                },
                { "while you have an? ({SkillMatchers}) summoned", Reference.AsSkill.Instances.Value > 0 },
                // - cast recently/in past x seconds
                { "if you've cast a spell recently,?", Skills[Keyword.Spell].Cast.Recently },
                { "if you've attacked recently,?", Skills[Keyword.Attack].Cast.Recently },
                { "if you've used a movement skill recently", Skills[Keyword.Movement].Cast.Recently },
                { "if you've used a minion skill recently", Minions.Cast.Recently },
                { "if you've used a warcry recently", Skills[Keyword.Warcry].Cast.Recently },
                { "if you've warcried recently", Skills[Keyword.Warcry].Cast.Recently },
                {
                    "if you've used a ({DamageTypeMatchers}) skill in the past # seconds",
                    Skills[Reference.AsDamageType].Cast.InPastXSeconds(Value)
                },
                { "if you summoned a golem in the past # seconds", Golems.Cast.InPastXSeconds(Value) },
                // traps and mines
                { "with traps", With(Keyword.Trap) },
                { "skills used by traps have", With(Keyword.Trap) },
                { "with mines", With(Keyword.Mine) },
                { "traps and mines (deal|have a)", Or(With(Keyword.Trap), With(Keyword.Mine)) },
                { "for throwing traps", With(Keyword.Trap) },
                { "if you detonated mines recently", Skills.DetonateMines.Cast.Recently },
                { "if you've placed a mine or thrown a trap recently", Or(Traps.Cast.Recently, Mines.Cast.Recently) },
                // totems
                { "totems (gain|have)", For(Entity.Totem) },
                { "you and your totems", Or(For(Self), For(Entity.Totem)) },
                { "totems fire", With(Keyword.Totem) },
                { "(spells cast|attacks used|skills used) by totems (have a|have)", With(Keyword.Totem) },
                { "of totem skills that cast an aura", And(With(Keyword.Totem), With(Keyword.Aura)) },
                { "while you have a totem", Totems.CombinedInstances.Value > 0 },
                { "if you've summoned a totem recently", Totems.Cast.Recently },
                // minions
                { "minions", For(Entity.Minion) },
                { "minions (deal|have|gain)", For(Entity.Minion) },
                { "you and your minions have", For(Entity.Minion).Or(For(Self)) },
                { "golems have", And(For(Entity.Minion), With(Keyword.Golem)) },
                { "spectres have", And(For(Entity.Minion), With(Skills.RaiseSpectre)) },
                { "skeletons deal", And(For(Entity.Minion), WithSkeletonSkills) },
                // flasks
                { "while using a flask", Flask.IsAnyActive },
                { "during any flask effect", Flask.IsAnyActive },
                // other
                { "against targets they pierce", Projectile.PierceCount.Value >= 1 },
                // unique
                { "while leeching", Condition.Unique("Are you leeching?") },
                {
                    "if you've killed an enemy affected by your damage over time recently",
                    Condition.Unique("Have you recently killed an Enemy affected by your Damage over Time?")
                },
                { "while you have energy shield", Condition.Unique("Do you have Energy Shield?") },
                {
                    "if you've taken fire damage from a hit recently",
                    Condition.Unique("Have you recently taken Fire Damage from a Hit?")
                },
            };
    }
}