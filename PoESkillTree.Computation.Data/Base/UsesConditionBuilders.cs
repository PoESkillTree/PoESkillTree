using System.Linq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.GameModel.Items;

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

        protected IConditionBuilder With(ISkillBuilder skill) => Condition.With(skill);

        protected IConditionBuilder WithSkeletonSkills
            => Or(With(Skills.SummonSkeleton), With(Skills.VaalSummonSkeletons));

        protected IConditionBuilder MainHandAttackWith(Tags tags) =>
            MainHandAttack.And(MainHand.Has(tags));

        protected IConditionBuilder OffHandAttackWith(Tags tags) =>
            OffHandAttack.And(OffHand.Has(tags));

        protected (IConditionBuilder mainHand, IConditionBuilder offHand) AttackWith(Tags tags) =>
            (MainHandAttackWith(tags), OffHandAttackWith(tags));

        protected IConditionBuilder MainHandAttack => Condition.AttackWith(AttackDamageHand.MainHand);
        protected IConditionBuilder OffHandAttack => Condition.AttackWith(AttackDamageHand.OffHand);

        protected IConditionBuilder ModifierSourceIs(ItemSlot slot)
            => Condition.ModifierSourceIs(new ModifierSource.Local.Item(slot));

        protected IConditionBuilder For(IEntityBuilder target) => Condition.For(target);

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

        protected IConditionBuilder EitherHandHas(Tags tags) =>
            Or(MainHand.Has(tags), OffHand.Has(tags));
    }
}