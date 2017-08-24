using System;
using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Parsing.Builders.Actions;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Entities;
using PoESkillTree.Computation.Parsing.Builders.Equipment;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Parsing.Builders.Skills
{
    public interface ISkillBuilderCollection : IBuilderCollection<ISkillBuilder>
    {
        // Returns a new collection with all skills in this collection that have all the keywords
        ISkillBuilderCollection this[params IKeywordBuilder[] keywords] { get; }

        // Returns a new collection with all skills in the item in the given slot
        ISkillBuilderCollection this[ItemSlot slot] { get; }
        ISkillBuilderCollection this[IItemSlotBuilder slot] { get; }

        IStatBuilder CombinedInstances { get; }

        // these apply to all skills in the collection

        IStatBuilder Duration { get; }

        IStatBuilder Cost { get; }
        IStatBuilder Reservation { get; }

        IStatBuilder CooldownRecoverySpeed { get; }

        IStatBuilder DamageEffectiveness { get; }

        IStatBuilder Speed { get; }

        IStatBuilder AreaOfEffect { get; }

        // If the skill has a stat with condition For(x), change that to For(x, target).
        // See "Your Offering Skills also affect you"
        IFlagStatBuilder ApplyStatsToEntity(IEntityBuilder entity);

        ISkillBuilderCollection Where(Func<ISkillBuilder, IConditionBuilder> predicate);

        // action for any of the skills
        IActionBuilder<ISelfBuilder, IEntityBuilder> Cast { get; }
    }
}