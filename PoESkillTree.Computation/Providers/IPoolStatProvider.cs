using System;

namespace PoESkillTree.Computation.Providers
{
    public interface IPoolStatProvider : IStatProvider
    {
        IRegenProvider Regen { get; }

        IRechargeProvider Recharge { get; }

        // both Regen and Recharge
        IStatProvider RecoveryRate { get; }

        // Reservation.Value returns percent reservation
        // .Minimum is 0, .Maximum is 100
        IStatProvider Reservation { get; }

        ILeechProvider Leech { get; }
        // Not in ILeechProvider because it does not convert with .AppliesTo() (e.g. Vaal Pact and Ghost Reaver)
        IFlagStatProvider InstantLeech { get; }

        // needs condition to have effect, e.g. "on kill", "on hit"
        IStatProvider Gain { get; }

        IConditionProvider IsFull { get; }
        IConditionProvider IsLow { get; }
    }


    public interface IRegenProvider : IStatProvider
    {
        // Percent will be added to stat behind IRegenProvider
        IStatProvider Percent { get; }

        // Set to 1 with Form.BaseSet for the pool stat from whose Regen property this instance originated.
        // If 1 (with Form.TotalOverride) for any other pool stat, that one applies.
        IFlagStatProvider AppliesTo(IPoolStatProvider stat);
    }


    public interface IRechargeProvider : IStatProvider
    {
        // 2 / RechargeStart.Value is the delay in seconds
        IStatProvider Start { get; } // default value: 1

        IConditionProvider StartedRecently { get; }
    }


    public interface ILeechProvider
    {
        IStatProvider Of(IDamageStatProvider damage);

        IStatProvider RateLimit { get; }
        IStatProvider Rate { get; }

        // Set to 1 with Form.BaseSet for the pool stat from whose Leech property this instance originated.
        // If 1 (with Form.TotalOverride) for any other pool stat, that one applies.
        IFlagStatProvider AppliesTo(IPoolStatProvider stat);

        // This is the entity that deals the damage by default. Can be changed leech to a different
        // target, e.g. Chieftain's "1% of Damage dealty by your Totems is Leeched to you as Life".
        ILeechProvider To(ITargetProvider target);

        // If set, all DamageStats from "Of(damage)" have their DamageType changed to the parameter
        IFlagStatProvider BasedOn(IDamageTypeProvider damageType);
    }


    public static class PoolStatProviders
    {
        public static readonly IPoolStatProvider Life;
        public static readonly IPoolStatProvider Mana;
        public static readonly IPoolStatProvider EnergyShield;

        public static IDamageTaken DamageTakenFrom(IPoolStatProvider pool)
        {
            throw new NotImplementedException();
        }

        public static IDamageTaken DamageTakenFrom(IDamageTypeProvider type, IPoolStatProvider pool)
        {
            throw new NotImplementedException();
        }


        public interface IDamageTaken
        {
            IStatProvider Before(IPoolStatProvider pool);
        }
    }
}