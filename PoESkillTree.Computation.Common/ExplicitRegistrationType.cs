using PoESkillTree.GameModel;
using PoESkillTree.Utils;

namespace PoESkillTree.Computation.Common
{
    /// <summary>
    /// Discriminated union for the type of explicitly registered stats.
    /// </summary>
    public abstract class ExplicitRegistrationType : ValueObject
    {
        private ExplicitRegistrationType()
        {
        }

        /// <summary>
        /// The stat's value must be specified by the user. E.g. conditions like "Is the enemy burning?".
        /// </summary>
        public sealed class UserSpecifiedValue : ExplicitRegistrationType
        {
            public UserSpecifiedValue(NodeValue? defaultValue)
                => DefaultValue = defaultValue;

            public NodeValue? DefaultValue { get; }

            protected override object ToTuple() => (GetType().Name, DefaultValue);
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

            protected override object ToTuple() => (GetType().Name, GainedStat, Action, ActionEntity);
        }

        /// <summary>
        /// The stat's value is tracked and might be useful to users, but is not used for other stats or accessible
        /// in <see cref="Builders"/>.
        /// </summary>
        public sealed class IndependentResult : ExplicitRegistrationType
        {
            public IndependentResult(NodeType resultType)
                => ResultType = resultType;

            /// <summary>
            /// The type of the node that should be displayed.
            /// </summary>
            public NodeType ResultType { get; }

            protected override object ToTuple() => (GetType().Name, ResultType);
        }
    }

    public static class ExplicitRegistrationTypes
    {
        public static ExplicitRegistrationType.UserSpecifiedValue UserSpecifiedValue(bool defaultValue)
            => UserSpecifiedValue((NodeValue?) defaultValue);

        public static ExplicitRegistrationType.UserSpecifiedValue UserSpecifiedValue(double defaultValue)
            => UserSpecifiedValue(new NodeValue(defaultValue));

        public static ExplicitRegistrationType.UserSpecifiedValue UserSpecifiedValue(NodeValue? defaultValue)
            => new ExplicitRegistrationType.UserSpecifiedValue(defaultValue);

        public static ExplicitRegistrationType.GainOnAction GainOnAction(
            IStat gainedStat, string action, Entity actionEntity)
            => new ExplicitRegistrationType.GainOnAction(gainedStat, action, actionEntity);

        public static ExplicitRegistrationType.IndependentResult IndependentResult(NodeType resultType)
            => new ExplicitRegistrationType.IndependentResult(resultType);
    }
}