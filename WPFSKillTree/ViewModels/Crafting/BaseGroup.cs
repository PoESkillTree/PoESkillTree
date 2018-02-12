using System;
using PoESkillTree.Common.Model.Items.Enums;
using POESKillTree.Model.Items.Enums;

namespace POESKillTree.ViewModels.Crafting
{
    /// <summary>
    /// Very rough classification of item/unique bases used for the first filter level in  
    /// <see cref="AbstractCraftingViewModel{TBase}"/>
    /// </summary>
    public enum BaseGroup
    {
        Any,
        OneHandWeapon,
        TwoHandWeapon,
        Armour,
        Other
    }


    public static class BaseGroupEx
    {
        /// <returns>true if the given tags match this group</returns>
        public static bool Matches(this BaseGroup baseGroup, Tags tags)
        {
            switch (baseGroup)
            {
                case BaseGroup.Any:
                    return true;
                case BaseGroup.OneHandWeapon:
                    return tags.HasFlag(Tags.OneHand);
                case BaseGroup.TwoHandWeapon:
                    return tags.HasFlag(Tags.TwoHand);
                case BaseGroup.Armour:
                    return tags.HasFlag(Tags.Armour);
                case BaseGroup.Other:
                    return !tags.HasFlag(Tags.OneHand)
                           && !tags.HasFlag(Tags.TwoHand)
                           && !tags.HasFlag(Tags.Armour);
                default:
                    throw new ArgumentOutOfRangeException(nameof(baseGroup), baseGroup, null);
            }
        }

        /// <returns>the group matching the given tags</returns>
        public static BaseGroup FromTags(Tags tags)
        {
            if (tags.HasFlag(Tags.OneHand))
            {
                return BaseGroup.OneHandWeapon;
            }
            if (tags.HasFlag(Tags.TwoHand))
            {
                return BaseGroup.TwoHandWeapon;
            }
            if (tags.HasFlag(Tags.Armour))
            {
                return BaseGroup.Armour;
            }
            return BaseGroup.Other;
        }
    }
}