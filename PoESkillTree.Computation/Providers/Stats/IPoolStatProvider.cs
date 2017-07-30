using PoESkillTree.Computation.Providers.Conditions;

namespace PoESkillTree.Computation.Providers.Stats
{
    public interface IPoolStatProvider : IStatProvider
    {
        IRegenStatProvider Regen { get; }

        IRechargeStatProvider Recharge { get; }

        // both Regen and Recharge
        IStatProvider RecoveryRate { get; }

        // Reservation.Value returns percent reservation
        // .Minimum is 0, .Maximum is 100
        IStatProvider Reservation { get; }

        ILeechStatProvider Leech { get; }
        // Not in ILeechProvider because it does not convert with .AppliesTo() (e.g. Vaal Pact and Ghost Reaver)
        IFlagStatProvider InstantLeech { get; }

        // needs condition to have effect, e.g. "on kill", "on hit"
        IStatProvider Gain { get; }

        // These conditions are either derived from Reservation.Value or user input
        // IsFull is true if Reservation.Value == 0 and user says this stat is full
        IConditionProvider IsFull { get; }
        // IsLow is true if Reservation.Value >= 65 or user says this stat is low
        IConditionProvider IsLow { get; }
    }
}