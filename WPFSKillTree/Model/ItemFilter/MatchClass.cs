using System;
using System.Collections.Generic;

namespace POESKillTree.Model.ItemFilter
{
    /*
     * Class:
     * Life Flasks
     * Mana Flasks
     * Hybrid Flasks
     * Currency
     * Amulets
     * Rings
     * Claws
     * Daggers
     * Wands
     * One Hand Swords
     * Thrusting One Hand Swords
     * One Hand Axes
     * One Hand Maces
     * Bows
     * Staves
     * Two Hand Swords
     * Two Hand Axes
     * Two Hand Maces
     * Active Skill Gems
     * Support Skill Gems
     * Quivers
     * Belts
     * Gloves
     * Boots
     * Body Armours
     * Helmets
     * Shields
     * Stackable Currency
     * Quest Items
     * Sceptres
     * Utility Flasks
     * Maps
     * Fishing Rods
     * Map Fragments
     * Divination Card
     * Jewel
     * Trinket
     */
    public class MatchClass : MatchStrings
    {
        public List<string> BaseTypes = new List<string>();

        protected MatchClass(MatchClass copy)
            : base(copy)
        {
            if (copy.BaseTypes != null)
                BaseTypes = new List<string>(copy.BaseTypes);
        }

        public MatchClass(string[] classes) : base(classes)
        {
            Keyword = "Class";
            Priority = Type.Class;
        }

        public override Match Clone()
        {
            return new MatchClass(this);
        }

        /// <summary>
        /// Learn BaseTypes which are subset of this class.
        /// </summary>
        /// <param name="match">The BaseType match to learn BaseTypes from.</param>
        public void Learn(MatchBaseType match)
        {
            foreach (string str in match.Values)
            {
                if (!BaseTypes.Contains(str)) BaseTypes.Add(str);
            }
        }
    }
}
