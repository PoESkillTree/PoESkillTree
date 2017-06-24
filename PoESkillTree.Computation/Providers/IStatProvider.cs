using System;

namespace PoESkillTree.Computation.Providers
{
    public interface IStatProvider
    {
        IStatProvider Maximum { get; }

        IConditionProvider ValueEquals(int value);
        IConditionProvider ValueEquals(IStatProvider stat);
    }

    public static class StatProviders
    {
        public static readonly IStatProvider Strength;
        public static readonly IStatProvider Dexterity;
        public static readonly IStatProvider Intelligence;

        public static readonly IStatProvider DamageEffectiveness;

        //public static readonly IStatProvider Damage;
        public static readonly IStatProvider CritMultiplier;
        public static readonly IStatProvider CritChance;
        public static readonly IStatProvider AttackSpeed;
        public static readonly IStatProvider Accuracy;
        public static readonly IStatProvider CastSpeed;

        public static readonly IStatProvider ChanceToBleed;

        public static readonly IStatProvider Life;
        public static readonly IStatProvider Mana;
        public static readonly IStatProvider Armour;
        public static readonly IStatProvider Evasion;
        public static readonly IStatProvider EnergyShield;

        //public static readonly IStatProvider Regen;
        //public static readonly IStatProvider Recharge;

        //public static readonly IStatProvider Resistance;
        public static readonly IStatProvider BlockChance;
        public static readonly IStatProvider PhysicalDamageReduction;

        public static readonly IStatProvider EnduranceCharges;
        public static readonly IStatProvider FrenzyCharges;
        public static readonly IStatProvider PowerCharges;

        public static IStatProvider Damage(IDamageSourceProvider source = null, IDamageTypeProvider type = null,
            IKeywordProvider keyword = null)
        {
            throw new NotImplementedException();
        }

        public static IStatProvider Regen(IStatProvider regenType)
        {
            throw new NotImplementedException();
        }

        public static IStatProvider Recharge(IStatProvider regenType)
        {
            throw new NotImplementedException();
        }

        public static IStatProvider Resistance(IDamageTypeProvider type)
        {
            throw new NotImplementedException();
        }

        // only this instead of the above? (usage: e.g. Stat(Damage, previous parameters))
        //public static IStatProvider Stat(IStatProvider baseStat, IDamageSourceProvider source = null, 
        //                          IDamageTypeProvider type = null,
        //                          IKeywordProvider keyword = null,
        //                          IStatProvider regenType = null);

        public static IStatProvider ApplyOnce(params IStatProvider[] stats)
        {
            throw new NotImplementedException();
        }
    }
}