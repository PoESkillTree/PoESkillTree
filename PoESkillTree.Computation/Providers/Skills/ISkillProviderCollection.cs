using System;
using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Providers.Actions;
using PoESkillTree.Computation.Providers.Conditions;
using PoESkillTree.Computation.Providers.Equipment;
using PoESkillTree.Computation.Providers.Entities;
using PoESkillTree.Computation.Providers.Stats;

namespace PoESkillTree.Computation.Providers.Skills
{
    public interface ISkillProviderCollection : IProviderCollection<ISkillProvider>
    {
        // Returns a new collection with all skills in this collection that have all the keywords
        ISkillProviderCollection this[params IKeywordProvider[] keywords] { get; }

        // Returns a new collection with all skills in the item in the given slot
        ISkillProviderCollection this[ItemSlot slot] { get; }
        ISkillProviderCollection this[IItemSlotProvider slot] { get; }

        IStatProvider CombinedInstances { get; }

        // these apply to all skills in the collection

        IStatProvider Duration { get; }

        IStatProvider Cost { get; }
        IStatProvider Reservation { get; }

        IStatProvider CooldownRecoverySpeed { get; }

        IStatProvider DamageEffectiveness { get; }

        IStatProvider Speed { get; }

        IStatProvider AreaOfEffect { get; }

        // If the skill has a stat with condition For(x), change that to For(x, target).
        // See "Your Offering Skills also affect you"
        IFlagStatProvider ApplyStatsToEntity(IEntityProvider entity);

        ISkillProviderCollection Where(Func<ISkillProvider, IConditionProvider> predicate);

        // action for any of the skills
        IActionProvider<ISelfProvider, IEntityProvider> Cast { get; }
    }
}