using PoESkillTree.Computation.Parsing.Builders.Conditions;

namespace PoESkillTree.Computation.Parsing.Builders.Stats
{
    public interface IPoolStatBuilder : IStatBuilder
    {
        IRegenStatBuilder Regen { get; }

        IRechargeStatBuilder Recharge { get; }

        // both Regen and Recharge
        IStatBuilder RecoveryRate { get; }

        // Reservation.Value returns percent reservation
        // .Minimum is 0, .Maximum is 100
        IStatBuilder Reservation { get; }

        ILeechStatBuilder Leech { get; }
        // Not in ILeechProvider because it does not convert with .AppliesTo() (e.g. Vaal Pact and Ghost Reaver)
        IFlagStatBuilder InstantLeech { get; }

        // needs condition to have effect, e.g. "on kill", "on hit"
        IStatBuilder Gain { get; }

        // These conditions are either derived from Reservation.Value or user input
        // IsFull is true if Reservation.Value == 0 and user says this stat is full
        IConditionBuilder IsFull { get; }
        // IsLow is true if Reservation.Value >= 65 or user says this stat is low
        IConditionBuilder IsLow { get; }
    }
}