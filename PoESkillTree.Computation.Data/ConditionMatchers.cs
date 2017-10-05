using System.Collections.Generic;
using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Parsing.Builders;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Data;
using PoESkillTree.Computation.Parsing.ModifierBuilding;

namespace PoESkillTree.Computation.Data
{
    public class ConditionMatchers : UsesMatchContext, IStatMatchers
    {
        private readonly IModifierBuilder _modifierBuilder;

        public ConditionMatchers(IBuilderFactories builderFactories, 
            IMatchContexts matchContexts, IModifierBuilder modifierBuilder) 
            : base(builderFactories, matchContexts)
        {
            _modifierBuilder = modifierBuilder;
        }

        public bool MatchesWholeLineOnly => false;

        public IEnumerable<MatcherData> Matchers => new ConditionMatcherCollection(
            _modifierBuilder)
        {
            // actions
            { "on ({ActionMatchers})", Group.AsAction.On() },
            { "if you've ({ActionMatchers}) recently", Group.AsAction.Recently() },
            { "if you haven't ({ActionMatchers}) recently", Not(Group.AsAction.Recently()) },
            {
                "when you ({ActionMatchers}) a rare or unique enemy",
                Group.AsAction.Against(Enemy).On(t => t.IsRareOrUnique)
            },
            { "on ({KeywordMatchers}) kill", Kill.On(withKeyword: Group.AsKeyword) },
            { "when you kill an enemy,", Kill.Against(Enemy).On() },
            {
                "if you've killed a maimed enemy recently",
                Kill.Against(Enemy).Recently(Buff.Maim.IsOn)
            },
            {
                "if you've killed a bleeding enemy recently",
                Kill.Against(Enemy).Recently(Ailment.Bleed.IsOn)
            },
            {
                "if you've killed a cursed enemy recently",
                Kill.Against(Enemy).Recently(
                    e => Buffs(target: e).With(Keyword.Curse).Any())
            },
            {
                "if you or your totems have killed recently",
                Or(Kill.Recently(), Kill.By(Entity.Totem).Recently())
            },
            { "when they block", Block.On() },
            { "when you block", Block.On() },
            {
                "if you've blocked a hit from a unique enemy recently",
                Block.Against(Enemy).Recently(s => s.IsUnique)
            },
            { "(from|with) hits", Hit.On() },
            { "hits deal", Hit.On() },
            { "when you are hit", Hit.Taken.On() },
            { "if you've been hit recently", Hit.Taken.Recently() },
            { "if you haven't been hit recently", Not(Hit.Taken.Recently()) },
            { "if you were damaged by a hit recently", Hit.Taken.Recently() },
            { "if you've taken no damage from hits recently", Not(Hit.Taken.Recently()) },
            { "if you've taken a savage hit recently", Action.SavageHit.Taken.Recently()  },
            { "when you deal a critical strike", CriticalStrike.On() },
            {
                "if you've crit in the past # seconds",
                CriticalStrike.InPastXSeconds(Value)
            },
            { "if you've shattered an enemy recently", Action.Shatter.Against(Enemy).Recently() },
            { "when you stun an enemy", Effect.Stun.On() },
            { "after spending # mana", Action.SpendMana(Value).On() },
            { "if you have consumed a corpse recently", Action.ConsumeCorpse.Recently() },
            // damage
            { "with weapons", Damage.With(Tags.Weapon) },
            { "weapon", Damage.With(Tags.Weapon) },
            { "with bows", Damage.With(Tags.Bow) },
            { "with swords", Damage.With(Tags.Sword) },
            { "with claws", Damage.With(Tags.Claw) },
            { "with daggers", Damage.With(Tags.Dagger) },
            { "with wands", Damage.With(Tags.Wand) },
            { "with axes", Damage.With(Tags.Axe) },
            { "with staves", Damage.With(Tags.Staff) },
            {
                "with maces",
                Or(Damage.With(Tags.Mace), Damage.With(Tags.Sceptre))
            },
            {
                "with one handed melee weapons",
                And(Damage.With(Tags.OneHandWeapon), Not(Damage.With(Tags.Ranged)))
            },
            {
                "with two handed melee weapons",
                And(Damage.With(Tags.TwoHandWeapon), Not(Damage.With(Tags.Ranged)))
            },
            { "with the main-hand weapon", Damage.With(ItemSlot.MainHand) },
            { "with main hand", Damage.With(ItemSlot.MainHand) },
            { "with off hand", Damage.With(ItemSlot.OffHand) },
            { "attacks have", Damage.With(Source.Attack) },
            { "from damage over time", Damage.With(Source.DamageOverTime) },
            {
                "with hits and ailments",
                Or(Hit.On(), Ailment.All.Any(Damage.With))
            },
            // action and damage combinations
            {
                "on melee hit",
                And(Hit.On(), With(Skills[Keyword.Melee]))
            },
            {
                "for each enemy hit by your attacks",
                And(Hit.Against(Enemy).On(), Damage.With(Source.Attack))
            },
            {
                "if you get a critical strike with a bow",
                And(CriticalStrike.On(), Damage.With(Tags.Bow))
            },
            {
                "if you get a critical strike with a staff",
                And(CriticalStrike.On(), Damage.With(Tags.Staff))
            },
            {
                "critical strikes with daggers have a",
                And(CriticalStrike.On(), Damage.With(Tags.Dagger))
            },
            {
                "on melee critical strike",
                And(CriticalStrike.On(), With(Skills[Keyword.Melee]))
            },
            {
                "projectiles have against targets they pierce",
                And(Projectile.Pierce.On(), With(Skills[Keyword.Projectile]))
            },
            // equipment
            { "while unarmed", Unarmed },
            { "while wielding a staff", MainHand.Has(Tags.Staff) },
            { "while dual wielding", OffHand.Has(Tags.Weapon) },
            { "while holding a shield", OffHand.Has(Tags.Shield) },
            {
                "while dual wielding or holding a shield",
                Or(OffHand.Has(Tags.Weapon), OffHand.Has(Tags.Shield))
            },
            { "with shields", OffHand.Has(Tags.Shield) },
            {
                "from equipped shield",
                And(Condition.BaseValueComesFrom(OffHand), OffHand.Has(Tags.Shield))
            },
            {
                "with # corrupted items equipped",
                Equipment.Count(e => e.IsCorrupted) >= Value
            },
            // stats
            { "when on low life", Life.IsLow },
            { "when not on low life", Not(Life.IsLow) },
            { "while no mana is reserved", Mana.Reservation.Value == 0 },
            { "while energy shield is full", EnergyShield.IsFull },
            { "while on full energy shield", EnergyShield.IsFull },
            { "while not on full energy shield", Not(EnergyShield.IsFull) },
            {
                "if energy shield recharge has started recently",
                EnergyShield.Recharge.StartedRecently
            },
            {
                "while you have no ({ChargeTypeMatchers})",
                Group.AsChargeType.Amount.Value == 0
            },
            {
                "while (at maximum|on full) ({ChargeTypeMatchers})",
                Group.AsChargeType.Amount.Value == Group.AsChargeType.Amount.Maximum.Value
            },
            {
                "if you have # primordial jewels,",
                Stat.PrimordialJewelsSocketed.Value >= Value
            },
            { "while you have ({FlagMatchers})", Group.AsFlagStat.IsSet },
            { "during onslaught", Flag.Onslaught.IsSet },
            { "while phasing", Flag.Phasing.IsSet },
            // stats on enemy
            { "(against enemies )?that are on low life", Enemy.Stat(Life).IsLow },
            { "(against enemies )?that are on full life", Enemy.Stat(Life).IsFull },
            { "against rare and unique enemies", Enemy.IsRareOrUnique },
            // buffs
            { "while you have fortify", Buff.Fortify.IsOn(Self) },
            { "if you've taunted an enemy recently", Buff.Taunt.Action.Recently() },
            { "enemies you taunt( deal)?", And(For(Enemy), Buff.Taunt.IsOn(Enemy)) },
            {
                "enemies you curse (take|have)",
                And(For(Enemy), Buffs(Self, Enemy).With(Keyword.Curse).Any())
            },
            { "(against|from) blinded enemies", Buff.Blind.IsOn(Enemy) },
            { "from taunted enemies", Buff.Taunt.IsOn(Enemy) },
            {
                "you and allies affected by your auras have",
                Or(For(Self),
                    And(For(Ally), Buffs(target: Ally).With(Keyword.Aura).Any()))
            },
            {
                "you and allies deal while affected by auras you cast",
                Or(For(Self),
                    And(For(Ally), Buffs(target: Ally).With(Keyword.Aura).Any()))
            },
            // ailments
            { "while ({AilmentMatchers})", Group.AsAilment.IsOn(Self) },
            { "(against|from) ({AilmentMatchers}) enemies", Group.AsAilment.IsOn(Enemy) },
            {
                "against frozen, shocked or ignited enemies",
                Or(Ailment.Freeze.IsOn(Enemy), Ailment.Shock.IsOn(Enemy), Ailment.Ignite.IsOn(Enemy))
            },
            { "enemies which are ({AilmentMatchers})", Group.AsAilment.IsOn(Enemy) },
            {
                "against enemies( that are)? affected by elemental ailments",
                Ailment.Elemental.Any(a => a.IsOn(Enemy))
            },
            {
                "against enemies( that are)? affected by no elemental ailments",
                Not(Ailment.Elemental.Any(a => a.IsOn(Enemy)))
            },
            {
                "poison you inflict with critical strikes deals",
                And(Damage.With(Ailment.Poison), CriticalStrike.On())
            },
            {
                "bleeding you inflict on maimed enemies deals",
                And(Damage.With(Ailment.Bleed), Buff.Maim.IsOn(Enemy))
            },
            {
                "with ({AilmentMatchers})", Damage.With(Group.AsAilment)
            },
            // ground effects
            { "while on consecrated ground", Ground.Consecrated.IsOn(Self) },
            // other effects
            { "against burning enemies", Fire.DamageOverTimeIsOn(Enemy) },
            // skills
            { "vaal( skill)?", With(Skills[Keyword.Vaal]) },
            { "({KeywordMatchers})", With(Skills[Group.AsKeyword]) },
            {
                "({KeywordMatchers}) and ({KeywordMatchers})",
                Or(With(Skills[Groups[0].AsKeyword]), With(Skills[Groups[1].AsKeyword]))
            },
            { "with ({KeywordMatchers}) skills", With(Skills[Group.AsKeyword]) },
            { "({KeywordMatchers}) skills have", With(Skills[Group.AsKeyword]) },
            { "of ({KeywordMatchers}) skills", With(Skills[Group.AsKeyword]) },
            { "for ({KeywordMatchers})", With(Skills[Group.AsKeyword]) },
            { "with traps", With(Traps) },
            { "with mines", With(Mines) },
            { "with ({DamageTypeMatchers}) skills", With(Skills[Group.AsDamageType]) },
            {
                "of totem skills that cast an aura",
                With(Skills[Keyword.Totem, Keyword.Aura])
            },
            {
                "({SkillMatchers})('|s)?( fires| has a| have a| has| deals|gain)?",
                With(Group.AsSkill)
            },
            {
                "skills (in|from) your ({ItemSlotMatchers})(can have| have)?",
                With(Skills[Group.AsItemSlot])
            },
            { "if you've cast a spell recently", Skills[Keyword.Spell].Cast.Recently() },
            { "if you've attacked recently", Skills[Keyword.Attack].Cast.Recently() },
            { "if you've used a movement skill recently", Skills[Keyword.Movement].Cast.Recently() },
            { "if you've used a warcry recently", Skills[Keyword.Warcry].Cast.Recently() },
            {
                "if you've used a ({DamageTypeMatchers}) skill in the past # seconds",
                Skills[Group.AsDamageType].Cast.InPastXSeconds(Value)
            },
            // traps and mines
            {
                "traps and mines (deal|have a)",
                Or(With(Traps), With(Mines))
            },
            {
                "from traps and mines",
                Or(With(Traps), With(Mines))
            },
            { "for throwing traps", With(Traps) },
            { "if you detonated mines recently", Skill.DetonateMines.Cast.Recently() },
            {
                "if you've placed a mine or thrown a trap recently",
                Or(Traps.Cast.Recently(), Mines.Cast.Recently())
            },
            // totems
            { "totems", For(Entity.Totem) },
            { "totems (fire|gain|have)", With(Totems) },
            { "(spells cast|attacks used|skills used) by totems (have a|have)", With(Totems) },
            { "while you have a totem", Totems.Any(s => s.HasInstance) },
            { "if you've summoned a totem recently", Totems.Cast.Recently() },
            { "when you place a totem", Totems.Cast.On() },
            // minions
            { "minions", For(Entity.Minion) },
            { "minions (deal|have|gain)", For(Entity.Minion) },
            { "you and your minions have", For(Entity.Minion, Self) },
            { "golem", For(Entity.Minion.With(Keyword.Golem)) },
            { "golems have", For(Entity.Minion.With(Keyword.Golem)) },
            { "spectres have", For(Entity.Minion.From(Skill.RaiseSpectre)) },
            {
                // technically this would be per minion summoned by that skill, but DPS will 
                // only be calculated for a single minion anyway
                "golems summoned in the past # seconds deal",
                With(Golems.Where(s => s.Cast.InPastXSeconds(Value)))
            },
            {
                "if you Summoned a golem in the past # seconds",
                Golems.Cast.InPastXSeconds(Value)
            },
            // flasks
            { "while using a flask", Flask.IsAnyActive },
            // other
            { "while leeching", Condition.WhileLeeching },
            { "(you )?gain", Condition.True }, // may be left over at the end, does nothing
            // unique
            { "when your trap is triggered by an enemy", Condition.Unique() },
            { "when your mine is detonated targeting an enemy", Condition.Unique() },
            { "when you gain a ({ChargeTypeMatchers})", Condition.Unique() },
            { "if you or your totems kill an enemy", Condition.Unique() },
            {
                "if you've (killed an enemy affected by your damage over time recently)",
                Condition.Unique("Have you $1?")
            },
        };
    }
}