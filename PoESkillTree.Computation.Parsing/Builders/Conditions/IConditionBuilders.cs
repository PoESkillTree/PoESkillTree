using PoESkillTree.Computation.Parsing.Builders.Entities;
using PoESkillTree.Computation.Parsing.Builders.Equipment;
using PoESkillTree.Computation.Parsing.Builders.Skills;

namespace PoESkillTree.Computation.Parsing.Builders.Conditions
{
    /// <summary>
    /// Factory interface for conditions.
    /// </summary>
    public interface IConditionBuilders
    {
        /// <summary>
        /// Gets a condition that is satisfied if Self is currently leeching.
        /// </summary>
        IConditionBuilder WhileLeeching { get; }

        /// <summary>
        /// Returns a condition that is satisfied if Self's current main skill is contained in
        /// <paramref name="skills"/>.
        /// </summary>
        IConditionBuilder With(ISkillBuilderCollection skills);

        /// <summary>
        /// Returns a conditions that is satisfied if Self's current main skill is <paramref name="skill"/>.
        /// </summary>
        IConditionBuilder With(ISkillBuilder skill);

        /// <summary>
        /// Returns a condition that is satisfied if Self is equivalent to any entity in <paramref name="entities"/>.
        /// If this method is not called when creating a modifier, a condition 
        /// <c>For(<see cref="IEntityBuilders.ModifierSource"/>)</c> is implicitly added. I.e. modifiers only apply
        /// to the entity they are gained from by default.
        /// </summary>
        /// <remarks>
        /// <c>For(<see cref="IEntityBuilders.Self"/>)</c> is always satisfied.
        /// <para>Can be used to apply stats to Enemy, e.g. "Enemies take 10% increased Damage".</para>
        /// <para>Minions have their own offensive and defensive stats. Modifiers only apply to minions when they
        /// have this condition (probably with some exceptions).</para>
        /// <para>Totems have their own defensive stats. Defensive modifiers only apply to totems when they have
        /// this condition.</para>
        /// </remarks>
        IConditionBuilder For(params IEntityBuilder[] entities);

        /// <summary>
        /// Returns a condition that is satisfied if the source of modified base values are the specified
        /// <paramref name="equipment"/>.
        /// </summary>
        IConditionBuilder BaseValueComesFrom(IEquipmentBuilder equipment);

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