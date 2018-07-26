using System;

namespace PoESkillTree.Computation.Common
{
    /// <summary>
    /// Discriminated union for the type of explicitly registered stats.
    /// </summary>
    public abstract class ExplicitRegistrationType : IEquatable<ExplicitRegistrationType>
    {
        private ExplicitRegistrationType()
        {
        }

        public sealed override bool Equals(object obj) => 
            (obj == this) || (obj is ExplicitRegistrationType other && Equals(other));

        public virtual bool Equals(ExplicitRegistrationType other) =>
            GetType() == other?.GetType();

        public override int GetHashCode() =>
            GetType().GetHashCode();

        public override string ToString() =>
            GetType().Name;

        /// <summary>
        /// The stat's value must be specified by the user. E.g. conditions like "Is the enemy burning?".
        /// </summary>
        public sealed class UserSpecifiedValue : ExplicitRegistrationType
        {
            public UserSpecifiedValue(double? defaultValue = null)
            {
                DefaultValue = defaultValue;
            }

            public double? DefaultValue { get; }

            public override bool Equals(ExplicitRegistrationType other) =>
                other is UserSpecifiedValue o &&
                DefaultValue.Equals(o.DefaultValue);

            public override int GetHashCode() =>
                (base.GetHashCode(), DefaultValue).GetHashCode();

            public override string ToString() =>
                base.ToString() + $"({DefaultValue})";
        }

        /// <summary>
        /// The stat's value is applied to <see cref="GainedStat"/> when <see cref="ActionEntity"/> does the action
        /// <see cref="Action"/>. This can be used to e.g. display life gain on hit/kill/...
        /// </summary>
        public sealed class GainOnAction : ExplicitRegistrationType
        {
            public GainOnAction(IStat gainedStat, string action, Entity actionEntity)
            {
                GainedStat = gainedStat;
                Action = action;
                ActionEntity = actionEntity;
            }

            public IStat GainedStat { get; }
            public string Action { get; }
            public Entity ActionEntity { get; }

            public override bool Equals(ExplicitRegistrationType other) =>
                other is GainOnAction o &&
                GainedStat.Equals(o.GainedStat) && Action == o.Action && ActionEntity == o.ActionEntity;

            public override int GetHashCode() =>
                (base.GetHashCode(), GainedStat, Action, ActionEntity).GetHashCode();

            public override string ToString() =>
                base.ToString() + $"({GainedStat}, {Action}, {ActionEntity})";
        }
    }

    public static class ExplicitRegistrationTypes
    {
        public static ExplicitRegistrationType.UserSpecifiedValue UserSpecifiedValue(double? defaultValue = null) =>
            new ExplicitRegistrationType.UserSpecifiedValue(defaultValue);

        public static ExplicitRegistrationType.GainOnAction GainOnAction(
            IStat gainedStat, string action, Entity actionEntity) =>
            new ExplicitRegistrationType.GainOnAction(gainedStat, action, actionEntity);
    }
}