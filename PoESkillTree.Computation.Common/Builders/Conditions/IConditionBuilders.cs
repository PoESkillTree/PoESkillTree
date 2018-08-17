using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.GameModel.Items;

namespace PoESkillTree.Computation.Common.Builders.Conditions
{
    /// <summary>
    /// Factory interface for conditions.
    /// </summary>
    public interface IConditionBuilders
    {
        /// <summary>
        /// Returns a condition that is satisfied if Self's current main skill has the given keyword.
        /// </summary>
        IConditionBuilder With(IKeywordBuilder keyword);

        /// <summary>
        /// Returns a condition that is satisfied if Self's current main skill is <paramref name="skill"/>.
        /// </summary>
        IConditionBuilder With(ISkillBuilder skill);

        /// <summary>
        /// Returns a condition that is satisfied if the damage related stat or action is done as an attack with
        /// the given hand.
        /// </summary>
        IConditionBuilder AttackWith(AttackDamageHand hand);

        /// <summary>
        /// Returns a condition that is satisfied if the damage related stat or action is done as the given source
        /// with a skill.
        /// </summary>
        IConditionBuilder With(DamageSource damageSource);

        IConditionBuilder DamageTaken { get; }

        /// <summary>
        /// Returns a condition that is satisfied if Self is equivalent to <paramref name="entity"/>.
        /// If this method is not called when creating a modifier, modifiers only apply
        /// to the entity they are gained from by default.
        /// </summary>
        /// <remarks>
        /// <c>For(<see cref="IEntityBuilders.Self"/>)</c> is always satisfied.
        /// <para>Can be used to apply stats to Enemy, e.g. "Enemies take 10% increased Damage".</para>
        /// <para>Minions have their own stats. Modifiers only apply to minions when they 
        /// have this condition (probably with some exceptions).</para>
        /// <para>Totems, mines and traps have their own non-damage stats.</para>
        /// </remarks>
        IConditionBuilder For(IEntityBuilder entity);

        /// <summary>
        /// Returns a condition that is satisfied if the source of modified base values is an item in the specified
        /// <paramref name="slot"/>.
        /// </summary>
        IConditionBuilder BaseValueComesFrom(ItemSlot slot);

        /// <summary>
        /// Returns a unique condition that is satisfied if explicitly set to be satisfied.
        /// </summary>
        /// <param name="name">The name the condition will be displayed with.</param>
        IConditionBuilder Unique(string name);

        /// <summary>
        /// Returns a condition that is always satisfied.
        /// </summary>
        IConditionBuilder True { get; }
    }
}