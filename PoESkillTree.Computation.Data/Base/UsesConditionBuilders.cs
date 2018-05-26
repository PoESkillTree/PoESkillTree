using System.Linq;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Skills;

namespace PoESkillTree.Computation.Data.Base
{
    /// <inheritdoc />
    /// <summary>
    /// Base class for matcher implementations providing direct access to <see cref="IConditionBuilders"/> and some of
    /// its methods.
    /// <para>Also contains convenience methods for the boolean operations on conditions</para>
    /// </summary>
    public abstract class UsesConditionBuilders : UsesStatBuilders
    {
        protected UsesConditionBuilders(IBuilderFactories builderFactories)
            : base(builderFactories)
        {
        }

        protected IConditionBuilders Condition => BuilderFactories.ConditionBuilders;

        protected IConditionBuilder With(IKeywordBuilder keyword) => Condition.With(keyword);

        protected IConditionBuilder With(IKeywordBuilder keyword, params IKeywordBuilder[] keywords) =>
            And(With(keyword), keywords.Select(With).ToArray());

        protected IConditionBuilder With(ISkillBuilder skill) => Condition.With(skill);

        protected IConditionBuilder For(params IEntityBuilder[] targets) => Condition.For(targets);

        /// <summary>
        /// Returns a condition that is satisfied if all given conditions are satisfied.
        /// </summary>
        protected static IConditionBuilder And(IConditionBuilder condition1, params IConditionBuilder[] conditions) =>
            conditions.Aggregate(condition1, (l, r) => l.And(r));

        /// <summary>
        /// Returns a condition that is satisfied if any of the given conditions is satisfied.
        /// </summary>
        protected static IConditionBuilder Or(IConditionBuilder condition1, params IConditionBuilder[] conditions) =>
            conditions.Aggregate(condition1, (l, r) => l.Or(r));

        /// <summary>
        /// Returns a condition that is satisfied if the given condition is not satisfied.
        /// </summary>
        protected static IConditionBuilder Not(IConditionBuilder condition) => condition.Not;
    }
}