using System;

namespace PoESkillTree.TreeGenerator.Model.PseudoAttributes
{
    /// <summary>
    /// Data class describing settings for the evaluation of conditions.
    /// </summary>
    public class ConditionSettings
    {
        public WeaponClass WeaponClass { get; private set; }

        public Tags Tags { get; private set; }

        public OffHand OffHand { get; private set; }
        
        /// <summary>
        /// Array of all keystones set in the skill tree.
        /// </summary>
        public string[] Keystones { get; private set; }

        public ConditionSettings(Tags tags, OffHand offHand, string[] keystones, WeaponClass weaponClass)
        {
            if (keystones == null) throw new ArgumentNullException("keystones");
            Tags = tags;
            OffHand = offHand;
            Keystones = keystones;
            WeaponClass = weaponClass;
        }
    }
}