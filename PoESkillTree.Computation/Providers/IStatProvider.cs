using System;

namespace PoESkillTree.Computation.Providers
{
    public interface IStatProvider
    {
        // Minimum has no effect if stat has default value 0 and no base modifiers (BaseSet or 
        // BaseAdd). That is necessary to make sure Unarmed and Incinerate can't crit as long they 
        // don't get base crit chance.
        IStatProvider Minimum { get; } // default value: negative infinity
        IStatProvider Maximum { get; } // default value: positive infinity

        ValueProvider Value { get; } // default: 0

        // returned stat has the converted percentage as value
        IStatProvider ConvertTo(IStatProvider stat);
        IStatProvider AddAs(IStatProvider stat);
        // All modifiers that do not have Form.BaseSet are also applied to stat at percentOfTheirValue
        IFlagStatProvider ApplyModifiersTo(IStatProvider stat, ValueProvider percentOfTheirValue);

        // chance to double Value
        IStatProvider ChanceToDouble { get; }

        IBuffProvider ForXSeconds(ValueProvider seconds);
        // similar to ForXSeconds(), just with the duration set elsewhere
        IBuffProvider AsBuff { get; }
        IFlagStatProvider AsAura { get; }

        // add stat to skills instead of the stat applying as is, 
        // e.g. "Auras you Cast grant ... to you and Allies"
        IFlagStatProvider AddTo(ISkillProviderCollection skills);
        // add stat to an effect, e.g. "Consecrated Ground you create grants ... to you and Allies"
        IFlagStatProvider AddTo(IEffectProvider effect);
    }


    // these can only have value 0 or 1
    public interface IFlagStatProvider : IStatProvider
    {
        // shortcut for Value == 1
        IConditionProvider IsSet { get; }

        IStatProvider EffectIncrease { get; }
        IStatProvider DurationIncrease { get; }
    }


    public interface IFlaskStatProvider
    {
        IStatProvider Effect { get; }
        IStatProvider Duration { get; }

        IStatProvider LifeRecovery { get; }
        IStatProvider ManaRecovery { get; }
        IStatProvider RecoverySpeed { get; }

        IStatProvider ChargesUsed { get; }
        IStatProvider ChargesGained { get; }

        IConditionProvider IsAnyActive { get;  }
    }


    public interface IProjectileStatProvider
    {
        IStatProvider Speed { get; }

        IStatProvider Count { get; }

        IStatProvider PierceCount { get; }
        IConditionProvider Pierces { get; }

        IStatProvider ChainCount { get; }

        // value is user entered, default is 35
        IStatProvider TravelDistance { get; }
    }


    public static class StatProviders
    {
        public static readonly IStatProvider Strength;
        public static readonly IStatProvider Dexterity;
        public static readonly IStatProvider Intelligence;
        public static readonly IStatProvider StrengthDamageBonus;
        public static readonly IStatProvider DexterityEvasionBonus;

        public static readonly IStatProvider Accuracy;
        public static readonly IStatProvider CritChance;
        public static readonly IStatProvider CritMultiplier;
        public static readonly IStatProvider AilmentCritMultiplier;
        // default value: 30% (default monster crit multi is 130%)
        public static readonly IStatProvider ExtraDamageFromCritsTaken;

        public static readonly IProjectileStatProvider Projectile;

        public static readonly IStatProvider Armour;
        public static readonly IStatProvider Evasion;

        // base values for these are calculated from other stats
        public static readonly IStatProvider ChanceToEvade;
        public static readonly IStatProvider ChanceToEvadeProjectileAttacks;
        public static readonly IStatProvider ChanceToEvadeMeleeAttacks;
        public static readonly IStatProvider ChanceToHit;

        public static readonly IStatProvider BlockChance;
        public static readonly IStatProvider BlockRecovery;
        public static readonly IStatProvider SpellBlockChance;
        public static readonly IStatProvider AttackDodgeChance;
        public static readonly IStatProvider SpellDodgeChance;

        // these have base values of 1 (so the value results in a multiplier)
        public static readonly IStatProvider AttackSpeed;
        public static readonly IStatProvider CastSpeed;
        public static readonly IStatProvider MovementSpeed;
        public static readonly IStatProvider AnimationSpeed;

        public static readonly IStatProvider TrapTriggerAoE;
        public static readonly IStatProvider MineDetonationAoE;

        public static readonly IStatProvider PrimordialJewelsSocketed;
        public static readonly IStatProvider GrandSpectrumJewelsSocketed;

        public static readonly IStatProvider ItemQuantity;
        public static readonly IStatProvider ItemRarity;

        public static readonly IStatProvider AreaOfEffect;
        public static readonly IStatProvider Range;

        public static readonly IStatProvider RampageStacks;

        public static readonly IFlaskStatProvider Flask;

        public static readonly IFlagStatProvider Onslaught;
        public static readonly IFlagStatProvider UnholyMight;
        public static readonly IFlagStatProvider Phasing;

        public static readonly IFlagStatProvider IgnoreMovementSpeedPenalties;

        // no "double dipping" if one of the stats is converted to another
        public static IStatProvider ApplyOnce(params IStatProvider[] stats)
        {
            throw new NotImplementedException();
        }
    }
}