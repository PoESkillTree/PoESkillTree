using PoESkillTree.Computation.Parsing.Builders.Entities;
using PoESkillTree.Computation.Parsing.Builders.Equipment;
using PoESkillTree.Computation.Parsing.Builders.Skills;

namespace PoESkillTree.Computation.Parsing.Builders.Conditions
{
    public interface IConditionBuilders
    {
        IConditionBuilder WhileLeeching { get; }

        IConditionBuilder With(ISkillBuilderCollection skills);
        IConditionBuilder With(ISkillBuilder skill);

        // Minions have their own offensive and defensive stats.
        // stats only apply to minions when they have this condition (probably with some exceptions)
        // Totems have their own defensive stats.
        // defensive stats only apply to totems when they have this condition
        // Can also be used to apply stats to Enemy instead of Self.
        IConditionBuilder For(params IEntityBuilder[] entities);

        // increases with this condition only affect base values coming from the specified equipment
        IConditionBuilder BaseValueComesFrom(IEquipmentBuilder equipment);

        // These need to be set by the user in a check box (will probably also need a section name 
        // or something) or are displayed as chances to gain something (as tool tip or something 
        // on the "Do you have X?" check box).
        // Name may be a regex replacement.
        IConditionBuilder Unique(string name = "$0");

        IConditionBuilder And(params IConditionBuilder[] conditions);
        IConditionBuilder Or(params IConditionBuilder[] conditions);
        IConditionBuilder Not(IConditionBuilder condition);

        IConditionBuilder True { get; }
    }
}