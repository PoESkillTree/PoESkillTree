using System;

namespace PoESkillTree.GameModel.Items
{
    public enum JewelRadius
    {
        None,
        Small,
        Medium,
        Large,
    }

    public static class JewelRadiusExtensions
    {
        public static uint GetRadius(this JewelRadius @this)
        {
            switch (@this)
            {
                case JewelRadius.None:
                    return 0;
                case JewelRadius.Small:
                    return 800;
                case JewelRadius.Medium:
                    return 1200;
                case JewelRadius.Large:
                    return 1500;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@this), @this, null);
            }
        }
    }
}