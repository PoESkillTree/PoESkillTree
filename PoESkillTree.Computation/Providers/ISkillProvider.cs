using System;
using PoESkillTree.Common.Model.Items.Enums;

namespace PoESkillTree.Computation.Providers
{
    public interface ISkillProvider
    {
        IActionProvider<ISelfProvider, ITargetProvider> Cast { get; }

        IStatProvider Instances { get; }
        // shortcut for Instances.Value > 0
        IConditionProvider HasInstance { get; }

        IStatProvider Duration { get; }

        IStatProvider Cost { get; }
        IStatProvider Reservation { get; }

        IStatProvider CooldownRecoverySpeed { get; }

        IStatProvider DamageEffectiveness { get; } // default value: 100

        // attack/cast rate (casts per second)
        IStatProvider Speed { get; }
    }

    public interface ISkillProviderCollection : IProviderCollection<ISkillProvider>
    {
        // Returns a new collection with all skills in this collection that have all the keywords
        ISkillProviderCollection this[params IKeywordProvider[] keywords] { get; }

        // Returns a new collection with all skills in the item in the given slot
        ISkillProviderCollection this[ItemSlot slot] { get; }
        ISkillProviderCollection this[IItemSlotProvider slot] { get; }

        IStatProvider CombinedInstances { get; }

        // these apply to all skils in the collection

        IStatProvider Duration { get; }

        IStatProvider Cost { get; }
        IStatProvider Reservation { get; }

        IStatProvider CooldownRecoverySpeed { get; }

        IStatProvider DamageEffectiveness { get; }

        IStatProvider Speed { get; }

        // If the skill has a stat with condition For(x), change that to For(x, target).
        // See "Your Offering Skills also affect you"
        IFlagStatProvider AddTargetToStats(ITargetProvider target);

        ISkillProviderCollection Where(Func<ISkillProvider, IConditionProvider> predicate);

        // action for any of the skills
        IActionProvider<ISelfProvider, ITargetProvider> Cast { get; }
    }

    public static class SkillProviders
    {
        public static readonly ISkillProvider SummonSkeletons;
        public static readonly ISkillProvider VaalSummonSkeletons;
        public static readonly ISkillProvider RaiseSpectre;
        public static readonly ISkillProvider RaiseZombie;

        public static readonly ISkillProvider DetonateMines;

        public static readonly ISkillProvider BloodRage;
        public static readonly ISkillProvider MoltenShell;

        public static ISkillProviderCollection Combine(params ISkillProvider[] skills)
        {
            throw new NotImplementedException();
        }

        public static ISkillProviderCollection Traps => Skills[KeywordProviders.TrapKeyword];
        public static ISkillProviderCollection Mines => Skills[KeywordProviders.MineKeyword];
        public static ISkillProviderCollection Totems => Skills[KeywordProviders.TotemKeyword];
        public static ISkillProviderCollection Golems => Skills[KeywordProviders.GolemKeyword];

        // all skills
        public static readonly ISkillProviderCollection Skills;
    }
}