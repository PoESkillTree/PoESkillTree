using PoESkillTree.Computation.Providers.Equipment;
using PoESkillTree.Computation.Providers.Skills;
using PoESkillTree.Computation.Providers.Entities;

namespace PoESkillTree.Computation.Providers.Conditions
{
    public interface IConditionProviderFactory
    {
        IConditionProvider WhileLeeching { get; }

        IConditionProvider With(ISkillProviderCollection skills);
        IConditionProvider With(ISkillProvider skill);

        // Minions have their own offensive and defensive stats.
        // stats only apply to minions when they have this condition (probably with some exceptions)
        // Totems have their own defensive stats.
        // defensive stats only apply to totems when they have this condition
        // Can also be used to apply stats to Enemy instead of Self.
        IConditionProvider For(params IEntityProvider[] entities);

        // increases with this condition only affect base values coming from the specified equipment
        IConditionProvider BaseValueComesFrom(IEquipmentProvider equipment);

        // These need to be set by the user in a check box (will probably also need a section name 
        // or something) or are displayed as chances to gain something (as tool tip or something 
        // on the "Do you have X?" check box).
        // Name may be a regex replacement.
        IConditionProvider Unique(string name = "$0");

        IConditionProvider And(params IConditionProvider[] conditions);
        IConditionProvider Or(params IConditionProvider[] conditions);
        IConditionProvider Not(IConditionProvider condition);

        IConditionProvider True { get; }
    }
}