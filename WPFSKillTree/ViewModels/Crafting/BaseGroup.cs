using System;
using POESKillTree.Model.Items.Enums;

namespace POESKillTree.ViewModels.Crafting
{
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
        public static bool Matches(this BaseGroup baseGroup, Tags tags)
        {
            switch (baseGroup)
            {
                case BaseGroup.Any:
                    return true;
                case BaseGroup.OneHandWeapon:
                    return tags.HasFlag(Tags.OneHandWeapon);
                case BaseGroup.TwoHandWeapon:
                    return tags.HasFlag(Tags.TwoHandWeapon);
                case BaseGroup.Armour:
                    return tags.HasFlag(Tags.Armour);
                case BaseGroup.Other:
                    return !tags.HasFlag(Tags.OneHandWeapon)
                           && !tags.HasFlag(Tags.TwoHandWeapon)
                           && !tags.HasFlag(Tags.Armour);
                default:
                    throw new ArgumentOutOfRangeException(nameof(baseGroup), baseGroup, null);
            }
        }

        public static BaseGroup FromTags(Tags tags)
        {
            if (tags.HasFlag(Tags.OneHandWeapon))
            {
                return BaseGroup.OneHandWeapon;
            }
            if (tags.HasFlag(Tags.TwoHandWeapon))
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