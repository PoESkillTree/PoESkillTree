using System.Collections.Generic;
using PoESkillTree.Common.Model.Items.Enums;

namespace PoESkillTree.Computation.Providers
{
    public interface IDamageTypeProvider : IKeywordProvider
    {
        // Combinations (And, Invert, Except) and IKeywordProvider:
        // matches gem/skill if it has any of the tags in the combination

        IDamageTypeProvider And(IDamageTypeProvider type);

        // e.g. fire -> (physical, lightning, cold, chaos)
        IDamageTypeProvider Invert { get; }

        // e.g. Elemental.Except(Fire) -> (Lightning, Cold)
        IDamageTypeProvider Except(IDamageTypeProvider type);

        IStatProvider Resistance { get; }

        IDamageStatProvider Damage { get; }

        IConditionProvider DamageOverTimeIsOn(ITargetProvider target);
    }


    public interface IDamageStatProvider : IStatProvider
    {
        IStatProvider Taken { get; }

        IStatProvider PenetrationOf(IDamageTypeProvider resistance);
        IFlagStatProvider IgnoreResistance(IDamageTypeProvider resistance);

        IDamageConditionProvider With { get; }
    }


    public interface IDamageConditionProvider : IConditionProvider
    {
        IDamageConditionProvider Source(IDamageSourceProvider source);

        IDamageConditionProvider Keyword(IKeywordProvider keyword);

        IDamageConditionProvider WeaponTags(Tags tags);

        IDamageConditionProvider Ailment(IAilmentProvider ailment);

        IDamageConditionProvider ItemSlot(ItemSlot slot);
    }


    public static class DamageTypeProviders
    {
        public static readonly IDamageTypeProvider Physical;
        public static readonly IDamageTypeProvider Fire;
        public static readonly IDamageTypeProvider Lightning;
        public static readonly IDamageTypeProvider Cold;
        public static readonly IDamageTypeProvider Elemental = Fire.And(Lightning).And(Cold);
        public static readonly IDamageTypeProvider Chaos;

        // For simplicity, the user needs to select the element
        public static readonly IDamageTypeProvider RandomElement;

        public static readonly IDamageTypeProvider AllDamage = Physical.And(Elemental).And(Chaos);

        public static readonly IDamageStatProvider Damage = AllDamage.Damage;

        public static readonly IReadOnlyList<IDamageTypeProvider> AllDamageTypes = new[]
        {
            Physical, Fire, Lightning, Cold, Chaos
        };
        public static readonly IReadOnlyList<IDamageTypeProvider> ElementalDamageTypes = new[]
        {
            Fire, Lightning, Cold
        };
    }
}